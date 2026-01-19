using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;
using AliceNeural.Utils;
using AliceNeural.Models;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Web;
using HttpProxyControl;
using System.Globalization;
using System.Text.Json.Serialization;
using Point = AliceNeural.Models.Point;
using AliceNeural.Models.LocalPath;
using AliceNeural.Models.LocalRec;
using Newtonsoft.Json.Linq;
using Permissions = Microsoft.Maui.ApplicationModel.Permissions;

namespace AliceNeural
{
    public partial class MainPage : ContentPage
    {
        #region variabili statiche
        static readonly HttpClient _client = HttpProxyHelper.CreateHttpClient(setProxy: true);
        static readonly string BingKey = "Ass_8WAQsXr7YXIsmBHQlhFcYwhn1_ljVgC8JJb9JtPyvqGsgeqKat3JN01vNiu-";
        SpeechRecognizer? speechRecognizer;
        IntentRecognizer? intentRecognizerByPatternMatching;
        IntentRecognizer? intentRecognizerByCLU;
        SpeechSynthesizer? speechSynthesizer;
        TaskCompletionSourceManager<int>? taskCompletionSourceManager;
        AzureCognitiveServicesResourceManager? serviceManager;
        bool buttonToggle = false;
        Brush? buttonToggleColor;
        private static readonly JsonSerializerOptions? jsonSerializationOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
        #endregion

        public MainPage()
        {
            InitializeComponent();
            serviceManager = new AzureCognitiveServicesResourceManager("MyResponder1", "AliceUpdated");
            taskCompletionSourceManager = new TaskCompletionSourceManager<int>();
            (intentRecognizerByPatternMatching, speechSynthesizer, intentRecognizerByCLU) =
                ConfigureContinuousIntentPatternMatchingWithMicrophoneAsync(
                    serviceManager.CurrentSpeechConfig,
                    serviceManager.CurrentCluModel,
                    serviceManager.CurrentPatternMatchingModel,
                    taskCompletionSourceManager);
            speechRecognizer = new SpeechRecognizer(serviceManager.CurrentSpeechConfig);
        }

        #region cose che non so cosa fanno
        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            if (speechSynthesizer != null)
            {
                await speechSynthesizer.StopSpeakingAsync();
                speechSynthesizer.Dispose();
            }

            if (intentRecognizerByPatternMatching != null)
            {
                await intentRecognizerByPatternMatching.StopContinuousRecognitionAsync();
                intentRecognizerByPatternMatching.Dispose();
            }

            if (intentRecognizerByCLU != null)
            {
                await intentRecognizerByCLU.StopContinuousRecognitionAsync();
                intentRecognizerByCLU.Dispose();
            }
        }

        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            await CheckAndRequestMicrophonePermission();
        }

        private async Task<PermissionStatus> CheckAndRequestMicrophonePermission()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
            if (status == PermissionStatus.Granted)
            {
                return status;
            }
            if (Permissions.ShouldShowRationale<Permissions.Microphone>())
            {
                // Prompt the user with additional information as to why the permission is needed
                await DisplayAlert("Permission required", "Microphone permission is necessary", "OK");
            }
            status = await Permissions.RequestAsync<Permissions.Microphone>();
            return status;
        }

        private static async Task ContinuousIntentPatternMatchingWithMicrophoneAsync(
            IntentRecognizer intentRecognizer, TaskCompletionSourceManager<int> stopRecognition)
        {
            await intentRecognizer.StartContinuousRecognitionAsync();
            // Waits for completion. Use Task.WaitAny to keep the task rooted.
            Task.WaitAny(new[] { stopRecognition.TaskCompletionSource.Task });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="cluModel"></param>
        /// <param name="patternMatchingModelCollection"></param>
        /// <param name="stopRecognitionManager"></param>
        /// <returns>una tupla contentente nell'ordine un intent recognizer basato su Patter Matching, un sintetizzatore vocale e un intent recognizer basato su un modello di Conversational Language Understanding </returns>
        private static (IntentRecognizer, SpeechSynthesizer, IntentRecognizer) ConfigureContinuousIntentPatternMatchingWithMicrophoneAsync(
            SpeechConfig config,
            ConversationalLanguageUnderstandingModel cluModel,
            LanguageUnderstandingModelCollection patternMatchingModelCollection,
            TaskCompletionSourceManager<int> stopRecognitionManager)
        {
            //creazione di un intent recognizer basato su pattern matching
            var intentRecognizerByPatternMatching = new IntentRecognizer(config);
            intentRecognizerByPatternMatching.ApplyLanguageModels(patternMatchingModelCollection);

            //creazione di un intent recognizer basato su CLU
            var intentRecognizerByCLU = new IntentRecognizer(config);
            var modelsCollection = new LanguageUnderstandingModelCollection { cluModel };
            intentRecognizerByCLU.ApplyLanguageModels(modelsCollection);

            //creazione di un sitetizzatore vocale
            var synthesizer = new SpeechSynthesizer(config);

            //gestione eventi
            intentRecognizerByPatternMatching.Recognized += async (s, e) =>
            {
                switch (e.Result.Reason)
                {
                    case ResultReason.RecognizedSpeech:
                        Debug.WriteLine($"PATTERN MATCHING - RECOGNIZED SPEECH: Text= {e.Result.Text}");
                        break;
                    case ResultReason.RecognizedIntent:
                        {
                            Debug.WriteLine($"PATTERN MATCHING - RECOGNIZED INTENT: Text= {e.Result.Text}");
                            Debug.WriteLine($"       Intent Id= {e.Result.IntentId}.");
                            if (e.Result.IntentId == "Ok")
                            {
                                Debug.WriteLine("Stopping current speaking if any...");
                                await synthesizer.StopSpeakingAsync();
                                Debug.WriteLine("Stopping current intent recognition by CLU if any...");
                                await intentRecognizerByCLU.StopContinuousRecognitionAsync();
                                await HandleOkCommand(synthesizer, intentRecognizerByCLU).ConfigureAwait(false);
                            }
                            else if (e.Result.IntentId == "Stop")
                            {
                                await synthesizer.StopSpeakingAsync();
                                Debug.WriteLine("Stopping current speaking...");

                            }
                        }

                        break;
                    case ResultReason.NoMatch:
                        Debug.WriteLine($"NOMATCH: Speech could not be recognized.");
                        var noMatch = NoMatchDetails.FromResult(e.Result);
                        switch (noMatch.Reason)
                        {
                            case NoMatchReason.NotRecognized:
                                Debug.WriteLine($"PATTERN MATCHING - NOMATCH: Speech was detected, but not recognized.");
                                break;
                            case NoMatchReason.InitialSilenceTimeout:
                                Debug.WriteLine($"PATTERN MATCHING - NOMATCH: The start of the audio stream contains only silence, and the service timed out waiting for speech.");
                                break;
                            case NoMatchReason.InitialBabbleTimeout:
                                Debug.WriteLine($"PATTERN MATCHING - NOMATCH: The start of the audio stream contains only noise, and the service timed out waiting for speech.");
                                break;
                            case NoMatchReason.KeywordNotRecognized:
                                Debug.WriteLine($"PATTERN MATCHING - NOMATCH: Keyword not recognized");
                                break;
                        }
                        break;

                    default:
                        break;
                }
            };
            intentRecognizerByPatternMatching.Canceled += (s, e) =>
            {
                Debug.WriteLine($"PATTERN MATCHING - CANCELED: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error)
                {
                    Debug.WriteLine($"PATTERN MATCHING - CANCELED: ErrorCode={e.ErrorCode}");
                    Debug.WriteLine($"PATTERN MATCHING - CANCELED: ErrorDetails={e.ErrorDetails}");
                    Debug.WriteLine($"PATTERN MATCHING - CANCELED: Did you update the speech key and location/region info?");
                }
                stopRecognitionManager.TaskCompletionSource.TrySetResult(0);
            };
            intentRecognizerByPatternMatching.SessionStopped += (s, e) =>
            {
                Debug.WriteLine("\n    Session stopped event.");
                stopRecognitionManager.TaskCompletionSource.TrySetResult(0);
            };

            return (intentRecognizerByPatternMatching, synthesizer, intentRecognizerByCLU);

        }
        #endregion

        #region switch intent recognized
        private static async Task HandleOkCommand(SpeechSynthesizer synthesizer, IntentRecognizer intentRecognizer)
        {
            await synthesizer.SpeakTextAsync("Sono in ascolto");
            //avvia l'intent recognition su Azure
            string? jsonResult = await RecognizeIntentAsync(intentRecognizer);
            if (jsonResult != null)
            {
                //process jsonResult
                //deserializzo il json
                CLUResponse cluResponse = JsonSerializer.Deserialize<CLUResponse>(jsonResult, jsonSerializationOptions) ?? new CLUResponse();
                await synthesizer.SpeakTextAsync($"La tua richiesta è stata {cluResponse.Result?.Query}");
                var topIntent = cluResponse.Result?.Prediction?.TopIntent;

                if (topIntent != null)
                {
                    switch (topIntent)
                    {
                        case string intent when intent.Contains("Wiki"):
                            //dichiarazioni casuali
                            string? argomento = "";
                            string[] categoria3 = new string[3];
                            string? subcategoria = "";
                            //for per prendere la città
                            foreach (var item in cluResponse?.Result?.Prediction?.Entities)
                            {
                                for (int i = 0; i < cluResponse.Result?.Prediction?.Entities?.Count; i++)
                                {
                                    categoria3[i] = item.Category;
                                    if (categoria3[i] == "Wiki.MainItemSearch")
                                    {
                                        argomento = item.Text;
                                    }
                                    else if (categoria3[i] == "Wiki.SubItemSearch")
                                    {
                                        subcategoria = item.Text;
                                    }
                                }
                            }
                            //faccio partire il metodo
                            await WikiIntent(argomento, subcategoria, synthesizer);


                            break;

                        case string intent when intent.Contains("Weather"):
                            //dichiarazioni casuali
                            string? citta = "";
                            string[] categoria = new string[3];
                            string? periodo = "";
                            string? condizione = "";
                            //for per prendere la città
                            foreach (var item in cluResponse?.Result?.Prediction?.Entities)
                            {
                                for (int i = 0; i < cluResponse.Result?.Prediction?.Entities?.Count; i++)
                                {
                                    categoria[i] = item.Category;
                                    if (categoria[i] == "Places.PlaceName")
                                    {
                                        citta = item.Text;
                                    }
                                    else if (categoria[i] == "datetimeV2")
                                    {
                                        periodo = item.Text;
                                    }
                                    else if (categoria[i] == "Weather.WeatherCondition")
                                    {
                                        condizione = item.Text;
                                    }
                                }
                            }
                            //faccio partire il metodo
                            await OpenMeteo(citta, periodo, condizione, synthesizer);

                            break;

                        case string intent when intent.Contains("Places"):
                            //dichiarazioni casuali
                            string? indirizzo = "";
                            string[] categoria2 = new string[3];
                            string? PostoArancione = "";
                            //for per prendere la città
                            foreach (var item in cluResponse?.Result?.Prediction?.Entities)
                            {
                                for (int i = 0; i < cluResponse.Result?.Prediction?.Entities?.Count; i++)
                                {
                                    categoria2[i] = item.Category;
                                    if (categoria2[i] == "Places.PlaceName" || categoria2[i] == "Places.AbsoluteLocation")
                                    {
                                        indirizzo = item.Text;
                                    }
                                    if (categoria2[i] == "Places.PlaceType")
                                    {
                                        PostoArancione = item.Text;
                                    }
                                }
                            }
                            //faccio partire il metodo
                            await BingMaps(indirizzo, PostoArancione, cluResponse.Result.Query, synthesizer);
                            break;

                        case string intent when intent.Contains("None"):
                            await synthesizer.SpeakTextAsync("Non ho capito");
                            break;
                    }

                }
                //determino l'action da fare, eventualmente effettuando una richiesta GET su un endpoint remoto scelto in base al topScoringIntent
                //ottengo il risultato dall'endpoit remoto
                //effettuo un text to speech per descrivere il risultato
            }
            else
            {
                //è stato restituito null - ad esempio quando il processo è interrotto prima di ottenre la risposta dal server
                Debug.WriteLine("Non è stato restituito nulla dall'intent reconition sul server");
            }
        }
        #endregion

        #region cose su bottone e intent
        public static async Task<string?> RecognizeIntentAsync(IntentRecognizer recognizer)
        {
            // Starts recognizing.
            Debug.WriteLine("Say something...");

            // Starts intent recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result. 
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query. 
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
            var result = await recognizer.RecognizeOnceAsync();
            string? languageUnderstandingJSON = null;

            // Checks result.
            switch (result.Reason)
            {
                case ResultReason.RecognizedIntent:
                    Debug.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Debug.WriteLine($"    Intent Id: {result.IntentId}.");
                    languageUnderstandingJSON = result.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult);
                    Debug.WriteLine($"    Language Understanding JSON: {languageUnderstandingJSON}.");
                    CLUResponse cluResponse = JsonSerializer.Deserialize<CLUResponse>(languageUnderstandingJSON, jsonSerializationOptions) ?? new CLUResponse();
                    Debug.WriteLine("Risultato deserializzato:");
                    Debug.WriteLine($"kind: {cluResponse.Kind}");
                    Debug.WriteLine($"result.query: {cluResponse.Result?.Query}");
                    Debug.WriteLine($"result.prediction.topIntent: {cluResponse.Result?.Prediction?.TopIntent}");
                    Debug.WriteLine($"result.prediction.Intents[0].Category: {cluResponse.Result?.Prediction?.Intents?[0].Category}");
                    Debug.WriteLine($"result.prediction.Intents[0].ConfidenceScore: {cluResponse.Result?.Prediction?.Intents?[0].ConfidenceScore}");
                    Debug.WriteLine($"result.prediction.entities: ");
                    cluResponse.Result?.Prediction?.Entities?.ForEach(s => Debug.WriteLine($"\tcategory = {s.Category}; text= {s.Text};"));
                    break;
                case ResultReason.RecognizedSpeech:
                    Debug.WriteLine($"RECOGNIZED: Text={result.Text}");
                    Debug.WriteLine($"    Intent not recognized.");
                    break;
                case ResultReason.NoMatch:
                    Debug.WriteLine($"NOMATCH: Speech could not be recognized.");
                    break;
                case ResultReason.Canceled:
                    var cancellation = CancellationDetails.FromResult(result);
                    Debug.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Debug.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Debug.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        Debug.WriteLine($"CANCELED: Did you update the subscription info?");
                    }
                    break;
            }
            return languageUnderstandingJSON;
        }
        private async void OnRecognitionButtonClicked2(object sender, EventArgs e)
        {
            if (serviceManager != null && taskCompletionSourceManager != null)
            {
                buttonToggle = !buttonToggle;
                if (buttonToggle)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        RecognizeSpeechBtn.Source = "microfono2.jpg";
                    });
                    //creo le risorse
                    //su un dispositivo mobile potrebbe succedere che cambiando rete cambino i parametri della rete, ed in particolare il proxy
                    //In questo caso, per evitare controlli troppo complessi, si è scelto di ricreare lo speechConfig ad ogni richiesta se cambia il proxy
                    if (serviceManager.ShouldRecreateSpeechConfigForProxyChange())
                    {
                        (intentRecognizerByPatternMatching, speechSynthesizer, intentRecognizerByCLU) =
                       ConfigureContinuousIntentPatternMatchingWithMicrophoneAsync(
                           serviceManager.CurrentSpeechConfig,
                           serviceManager.CurrentCluModel,
                           serviceManager.CurrentPatternMatchingModel,
                           taskCompletionSourceManager);
                    }

                    _ = Task.Factory.StartNew(async () =>
                    {
                        taskCompletionSourceManager.TaskCompletionSource = new TaskCompletionSource<int>();
                        await ContinuousIntentPatternMatchingWithMicrophoneAsync(
                            intentRecognizerByPatternMatching!, taskCompletionSourceManager)
                        .ConfigureAwait(false);
                    });
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        RecognizeSpeechBtn.Source = "microfono.jpg";
                    });
                    //la doppia chiamata di StopSpeakingAsync è un work-around a un problema riscontrato in alcune situazioni:
                    //se si prova a fermare il task mentre il sintetizzatore sta parlando, in alcuni casi si verifica un'eccezione. 
                    //Con il doppio StopSpeakingAsync non succede.
                    await speechSynthesizer!.StopSpeakingAsync();
                    await speechSynthesizer.StopSpeakingAsync();
                    await intentRecognizerByCLU!.StopContinuousRecognitionAsync();
                    await intentRecognizerByPatternMatching!.StopContinuousRecognitionAsync();
                    //speechSynthesizer.Dispose();
                    //intentRecognizerByPatternMatching.Dispose();
                }
            }
        }
        private async void OnRecognitionButtonClicked(object sender, EventArgs e)
        {
            try
            {
                //accedo ai servizi
                //AzureCognitiveServicesResourceManager serviceManager =(Application.Current as App).AzureCognitiveServicesResourceManager;
                // Creates a speech recognizer using microphone as audio input.
                // Starts speech recognition, and returns after a single utterance is recognized. The end of a
                // single utterance is determined by listening for silence at the end or until a maximum of 15
                // seconds of audio is processed.  The task returns the recognition text as result.
                // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
                // shot recognition like command or query.
                // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
                var result = await speechRecognizer!.RecognizeOnceAsync().ConfigureAwait(false);

                // Checks result.
                StringBuilder sb = new();
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    sb.AppendLine($"RECOGNIZED: Text={result.Text}");
                    await speechSynthesizer!.SpeakTextAsync(result.Text);
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    sb.AppendLine($"NOMATCH: Speech could not be recognized.");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    sb.AppendLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        sb.AppendLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        sb.AppendLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                        sb.AppendLine($"CANCELED: Did you update the subscription info?");
                    }
                }
                UpdateUI(sb.ToString());
            }
            catch (Exception ex)
            {
                UpdateUI("Exception: " + ex.ToString());
            }
        }
        private void UpdateUI(String message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RecognitionText.Text = message;
            });
        }
        #endregion

        #region prendo coordinate
        //public static async Task GetLocationAsync()
        //{
        //    // Ottieni l'ultima posizione conosciuta
        //    var location = await Geolocation.GetLastKnownLocationAsync();

        //    if (location != null)
        //    {
        //        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
        //        return;
        //    }

        //    // Se l'ultima posizione conosciuta non è disponibile, ottieni una nuova posizione
        //    location = await Geolocation.GetLocationAsync(new GeolocationRequest
        //    {
        //        DesiredAccuracy = GeolocationAccuracy.Medium,
        //        Timeout = TimeSpan.FromSeconds(30)
        //    });

        //    if (location != null)
        //    {
        //        Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}");
        //    }
        //}
        public static async Task<(double? lat, double? lon)?> GetCoordinate(string? città, string language = "it", int count = 1)
        {
            string? cittaCod = HttpUtility.UrlEncode(città);
            string urlCoordinate = $"https://geocoding-api.open-meteo.com/v1/search?name={cittaCod}&count={count}&language={language}";
            try
            {
                HttpResponseMessage response = await _client.GetAsync($"{urlCoordinate}");
                if (response.IsSuccessStatusCode)
                {
                    //await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
                    GeoCoding? geoCoding = await response.Content.ReadFromJsonAsync<GeoCoding>();
                    if (geoCoding != null && geoCoding.Results?.Count > 0)
                    {
                        return (geoCoding.Results[0].Latitude, geoCoding.Results[0].Longitude);
                    }
                }
                return null;
            }
            catch (Exception)
            {

                Debug.WriteLine("Errore");
            }
            return null;
        }
        #endregion

        #region OpenMeteo
        private static async Task OpenMeteo(string città, string periodo, string condizione, SpeechSynthesizer synthesizer)
        {

            const string datoNonFornitoString = "";
            var geo = await GetCoordinate(città);
            if (geo != null)
            {
                int counterGiorni = 1;
                DateOnly data = DateOnly.FromDateTime(dateTime: DateTime.Today);
                DateOnly data2 = DateOnly.FromDateTime(dateTime: DateTime.Today);
                if (periodo == "Domani" || periodo == "domani")
                {
                    data = data.AddDays(1);
                    data2 = data;
                    counterGiorni += 1;
                }
                else if (periodo == "oggi" || periodo == "Oggi")
                {
                    data2 = data;
                }
                else if (periodo == "Dopodomani" || periodo == "dopodomani")
                {
                    data = data.AddDays(2);
                    data2 = data;
                    counterGiorni += 2;
                }
                else if (periodo == null || periodo == "")
                {
                    if (condizione != null && condizione != "")
                    {
                        data2 = data2.AddDays(6);
                        counterGiorni += 6;
                    }
                }
                else
                {
                    data = DateOnly.Parse($"{periodo}");
                    periodo = $"il {periodo}";
                    data2 = data;
                }
                string datazione = $"{data.ToString("o", CultureInfo.InvariantCulture)}";
                string datazione2 = $"{data2.ToString("o", CultureInfo.InvariantCulture)}";
                datazione = datazione.Replace("/", "-");
                datazione2 = datazione2.Replace("/", "-");
                FormattableString addressUrlFormattable;
                addressUrlFormattable = $"https://api.open-meteo.com/v1/forecast?latitude={geo?.lat}&longitude={geo?.lon}&hourly=temperature_2m,relative_humidity_2m&daily=weather_code,temperature_2m_max,temperature_2m_min,apparent_temperature_max,apparent_temperature_min,precipitation_sum,rain_sum,showers_sum,snowfall_sum&timezone=auto&start_date={datazione}&end_date={datazione2}";
                DateTime now = DateTime.Now;
                string ora = now.ToString("HH:mm");
                ora = ora.Remove(2);
                int ore = int.Parse(ora);
                string addressUrl = FormattableString.Invariant(addressUrlFormattable);
                var response = await _client.GetAsync($"{addressUrl}");
                if (response.IsSuccessStatusCode)
                {
                    Root? forecast = await response.Content.ReadFromJsonAsync<Root>();
                    if (forecast != null)
                    {
                        if (condizione != null && condizione != "" && condizione != "tempo" && condizione != "Tempo")
                        {
                            int count = 1;
                            int contatoreFor = 0;
                            DateOnly dataVera = DateOnly.FromDateTime(dateTime: DateTime.Today);
                            dataVera = data;
                            string[] giornateEvento = new string[7];
                            int checkpioggia = 0;
                            int checkneve = 0;
                            int checkumidità = 0;
                            int umiditàComplessiva = 0;
                            int umiditàTot = 0;
                            foreach (var item in forecast.Daily.WeatherCode)
                            {
                                if (condizione == "pioverà" || condizione == "Pioverà")
                                {
                                    if (item >= 51 && item <= 65 || item >= 80 && item <= 99)
                                    {
                                        giornateEvento[contatoreFor] = dataVera.ToLongDateString();
                                        checkpioggia = 1;
                                    }
                                }
                                else if (condizione == "nevicherà" || condizione == "Nevicherà")
                                {
                                    if (item > 65 && item < 80)
                                    {
                                        giornateEvento[contatoreFor] = dataVera.ToLongDateString();
                                        checkneve = 1;
                                    }
                                }
                                else if (condizione == "umidità" || condizione == "Umidità")
                                {
                                    checkumidità = 1;
                                    if (data != data2)
                                    {
                                    }
                                    else
                                    {
                                        foreach (var item2 in forecast.Hourly.RelativeHumidity2m)
                                        {
                                            umiditàComplessiva += item2;
                                        }
                                        umiditàTot = umiditàComplessiva / 24;
                                    }
                                }
                                else
                                {
                                }
                                dataVera = dataVera.AddDays(1);
                                count++;
                                contatoreFor++;
                            }
                            if (checkpioggia == 1)
                            {
                                if (periodo != null && periodo != "")
                                {
                                    await synthesizer.SpeakTextAsync($"si, {periodo} {condizione} e ci saranno {forecast.Daily.RainSum[0]} mm di pioggia");
                                }
                                else
                                {
                                    await synthesizer.SpeakTextAsync($"a {città} {condizione}");
                                    foreach (var item in giornateEvento)
                                    {
                                        await synthesizer.SpeakTextAsync($"{item}");
                                    }
                                }
                            }
                            else if (checkneve == 1)
                            {
                                if (periodo != null && periodo != "")
                                {
                                    await synthesizer.SpeakTextAsync($"si, {periodo} {condizione} e ci saranno {forecast.Daily.RainSum[0]} mm di neve");
                                }
                                else
                                {
                                    await synthesizer.SpeakTextAsync($"a {città} {condizione}");
                                    foreach (var item in giornateEvento)
                                    {
                                        await synthesizer.SpeakTextAsync($"{item}");
                                    }
                                }
                            }
                            else if (checkumidità == 1)
                            {
                                if (data != data2)
                                {
                                    await synthesizer.SpeakTextAsync($"periodo raccolta dati troppo grande, provare con domani");
                                }
                                else
                                {
                                    await synthesizer.SpeakTextAsync($"{periodo} a {città} ci sarà una {condizione} media del {umiditàTot} %");
                                }
                            }
                            else
                            {
                                if (condizione == "temperatura" || condizione == "Temperatura")
                                {
                                    await synthesizer.SpeakTextAsync($"{periodo} a  {città} " +
                                    $"il meteo sarà {Utilss.Display(Utilss.WMOCodesIntIT(forecast.Daily.WeatherCode[0]), datoNonFornitoString)} " +
                                    $" con una massima di {forecast.Daily.Temperature2mMax.First()}° e una minima di {forecast.Daily.Temperature2mMin.First()}°");
                                    if (forecast.Daily.RainSum[0] != 0.00)
                                    {
                                        await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.RainSum[0]} mm di pioggia");
                                    }
                                    else if (forecast.Daily.ShowersSum[0] != 0.00)
                                    {
                                        await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.ShowersSum[0]} mm di pioggia");
                                    }
                                    else if (forecast.Daily.SnowfallSum[0] != 0.00)
                                    {
                                        await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.SnowfallSum[0]} mm di neve");
                                    }
                                }
                                else if (data != data2)
                                {
                                    await synthesizer.SpeakTextAsync($"in questo periodo non {condizione} a {città} ma" +
                                    $"il meteo sarà {Utilss.Display(Utilss.WMOCodesIntIT(forecast.Daily.WeatherCode[0]), datoNonFornitoString)} " +
                                    $" con una massima di {forecast.Daily.Temperature2mMax.First()}° e una minima di {forecast.Daily.Temperature2mMin.First()}°");
                                }
                                else
                                {
                                    await synthesizer.SpeakTextAsync($"no, {periodo} a  {città} " +
                                    $"il meteo sarà {Utilss.Display(Utilss.WMOCodesIntIT(forecast.Daily.WeatherCode[0]), datoNonFornitoString)} " +
                                    $" con una massima di {forecast.Daily.Temperature2mMax.First()}° e una minima di {forecast.Daily.Temperature2mMin.First()}°");
                                    if (forecast.Daily.RainSum[0] != 0.00)
                                    {
                                        await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.RainSum[0]} mm di pioggia");
                                    }
                                    else if (forecast.Daily.ShowersSum[0] != 0.00)
                                    {
                                        await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.ShowersSum[0]} mm di pioggia");
                                    }
                                    else if (forecast.Daily.SnowfallSum[0] != 0.00)
                                    {
                                        await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.SnowfallSum[0]} mm di neve");
                                    }
                                }
                            }

                        }
                        else
                        {
                            await synthesizer.SpeakTextAsync($"{periodo} a  {città} " +
                                $"il meteo sarà {Utilss.Display(Utilss.WMOCodesIntIT(forecast.Daily.WeatherCode[0]), datoNonFornitoString)} " +
                                    $" con una massima di {forecast.Daily.Temperature2mMax.First()}° e una minima di {forecast.Daily.Temperature2mMin.First()}°");
                            if (forecast.Daily.RainSum[0] != 0.00)
                            {
                                await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.RainSum[0]} mm di pioggia");
                            }
                            else if (forecast.Daily.ShowersSum[0] != 0.00)
                            {
                                await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.ShowersSum[0]} mm di pioggia");
                            }
                            else if (forecast.Daily.SnowfallSum[0] != 0.00)
                            {
                                await synthesizer.SpeakTextAsync($"{periodo} ci saranno {forecast.Daily.SnowfallSum[0]} mm di neve");
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region bingmaps
        static async Task FindPointOfInterest(string url, string posto, double radius, SpeechSynthesizer synthesizer)
        {
            // recupara coordinate geografiche
            var point = await GetCoordinate(url);
            string? lat = $"{point?.lat}";
            string? lng = $"{point?.lon}";
            lat = lat.Replace(",", ".");
            lng = lng.Replace(",", ".");
            FormattableString urlComplete = $"https://dev.virtualearth.net/REST/v1/LocationRecog/{lat},{lng}?radius={radius}&top=15&datetime=2024-04-11%2018:50:42Z&distanceunit=km&verboseplacenames=true&includeEntityTypes=businessAndPOI,naturalPOI,address&includeNeighborhood=1&include=ciso2&key={BingKey}";

            string Ristoranti = "Buffet Restaurants Cafe Restaurants Chinese Restaurants Diners Ice Cream And Frozen Desserts Italian Restaurants Japanese Restaurants Mexican Restaurants Pizza Restaurants Sandwiches Seafood Restaurants Steak House Restaurants Sushi Restaurants Take Away Taverns Vegetarian And VeganRestaurants";
            string Bar = "Bars Bars Grills And Pubs Desserts Ice Cream Parlors Cocktail Lounges Coffee And Tea Sports Bars B2B Agriculture and Food B2B Food Products";
            string Supermercati = "Supermarkets Liquor Stores Grocery Grocers Discount Stores Fish and Meat Markets Farmers Markets";
            string Negozi = "Construction Services Automotive and Vehicles Cars and Trucks Book stores Real Estate Rental Services Beauty and Spa Business-to-Business Health and Beauty Supplies CD And Record Stores Cigar And Tobacco Shops Discount Stores Furniture Stores Home Improvement Stores Jewelry And Watches Stores Liquor Stores Malls And Shopping Centers Music Stores Outlet Stores Pet Shops Pet Supply Stores School And Office Supply Stores Shoe Stores Sporting Goods Stores Toy And Game Stores";
            string Attrazioni = "Amusement Parks Attractions Carnivals Casinos Landmarks And Historical Sites Movie Theaters Museums Parks Zoos";
            string? FastFood = "Fast Food";
            string? Hospitals = "Hospitals";
            string? Hotel = "Hotels And Motels";
            string? Parcheggi = "Parking";

            string addressUrl = FormattableString.Invariant(urlComplete);
            HttpResponseMessage response = await _client.GetAsync(addressUrl);
            if (response.IsSuccessStatusCode)
            {
                if (posto != "" && posto != null)
                {
                    LocalRecognition? data = await response.Content.ReadFromJsonAsync<LocalRecognition>();
                    int? numeroPunti = data?.ResourceSets?[0]?.Resources?[0]?.BusinessesAtLocation?.Count;
                    var resources = data?.ResourceSets?[0]?.Resources?[0];

                    if (posto == "Ristoranti" || posto == "ristoranti")
                    {
                        await synthesizer.SpeakTextAsync($"come ristoranti troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (Ristoranti.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else if (posto == "Bar" || posto == "bar")
                    {
                        await synthesizer.SpeakTextAsync($"come bar troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (Bar.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                        Debug.WriteLine(dettato);
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else if (posto == $"negozi" || posto == "Negozi")
                    {
                        await synthesizer.SpeakTextAsync($"come negozi troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (Negozi.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else if (posto == $"Supermercati" || posto == "supermercati")
                    {
                        await synthesizer.SpeakTextAsync($"come supermercati troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (Supermercati.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else if (posto == $"Musei" || posto == "musei" || posto == "Cinema" || posto == "cinema" || posto == "Attrazioni" || posto == "attrazioni" || posto == "casinò" || posto == "Casinò" || posto == "Parchi" || posto == "parchi")
                    {
                        await synthesizer.SpeakTextAsync($"come attrazioni troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (Attrazioni.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else if (posto == $"Fastfood" || posto == "fastfood")
                    {
                        await synthesizer.SpeakTextAsync($"come fast food troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (FastFood.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else if (posto == $"ospedali" || posto == "Ospedali")
                    {
                        await synthesizer.SpeakTextAsync($"come ospedali troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (Hospitals.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else if (posto == $"Hotel" || posto == "hotel")
                    {
                        await synthesizer.SpeakTextAsync($"come hotel troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (Hotel.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else if (posto == $"Parcheggi" || posto == "parcheggi")
                    {
                        await synthesizer.SpeakTextAsync($"come parcheggi troviamo:");
                        string dettato = "";
                        for (int i = 0; i < numeroPunti; i++)
                        {
                            for (int z = 0; z < resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?.Count; z++)
                            {
                                if (Parcheggi.Contains(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.OtherTypes?[z]))
                                {
                                    if (dettato != resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName)
                                    {
                                        dettato = $"{resources?.BusinessesAtLocation?[i]?.BusinessInfo?.EntityName}";
                                        await synthesizer.SpeakTextAsync($"{dettato}");
                                    }
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Type);
                                    //Debug.WriteLine(resources?.BusinessesAtLocation?[i]?.BusinessInfo?.Phone);
                                }
                            }
                        }
                        if (dettato == "")
                        {
                            await synthesizer.SpeakTextAsync($"non ho trovato {posto} a {url}");
                        }
                    }

                    else
                    {
                        await synthesizer.SpeakTextAsync($"non ci sono {posto} a {url}");
                    }
                }
                else
                {
                    LocalRecognition? data = await response.Content.ReadFromJsonAsync<LocalRecognition>();
                    Debug.WriteLine(data.ResourceSets[0].Resources[0].BusinessesAtLocation[0].BusinessInfo.EntityName.ToString());
                    int numeroPunti = data.ResourceSets[0].Resources[0].BusinessesAtLocation.Count;
                    var resources = data.ResourceSets[0].Resources[0];
                    await synthesizer.SpeakTextAsync($"a {url} c'è:");
                    for (int i = 0; i < numeroPunti; i++)
                    {
                        await synthesizer.SpeakTextAsync(resources.BusinessesAtLocation[i].BusinessInfo.EntityName);
                        await synthesizer.SpeakTextAsync("Tipologia: ");
                        await synthesizer.SpeakTextAsync(resources.BusinessesAtLocation[i].BusinessInfo.Type);
                        //Debug.WriteLine(resources.BusinessesAtLocation[i].BusinessInfo.Phone);
                        //Debug.WriteLine("*********************");
                    }
                }
            }
        }
        static async Task RouteWp1ToWp2(string wp1, string wp2)
        {
            string wp1Encode = HttpUtility.UrlEncode(wp1);
            string wp2Encode = HttpUtility.UrlEncode(wp2);
            string urlCompleto = $"https://dev.virtualearth.net/REST/v1/Routes?wp.1={wp1Encode}&wp.2={wp2Encode}&optimize=time&tt=departure&dt=2024-04-11%2019:35:00&distanceUnit=km&c=it&ra=regionTravelSummary&key={BingKey}";
            HttpResponseMessage response = await _client.GetAsync(urlCompleto);
            if (response.IsSuccessStatusCode)
            {
                LocalRoute? localRoute = await response.Content.ReadFromJsonAsync<LocalRoute>();
                if (localRoute != null)
                {
                    // distanza in km
                    double? distanza = localRoute.ResourceSets[0].Resources[0].TravelDistance;
                    double durata = localRoute.ResourceSets[0].Resources[0].TravelDuration;
                    double durataConTraffico = localRoute.ResourceSets[0].Resources[0].TravelDurationTraffic;
                    string modViaggio = localRoute.ResourceSets[0].Resources[0].TravelMode;
                    Debug.WriteLine($"La distanza da {wp1} a {wp2}  è di {distanza} KM" +
                        $"\ncon una durata di {durata / 60} minuti o " +
                        $"con {durataConTraffico / 60} minuti con il traffico attuale utilizzando {modViaggio} ");
                    Debug.WriteLine("*****************");
                    for (int i = 0; i < localRoute.ResourceSets[0].Resources[0].RouteLegs[0].ItineraryItems.Count; i++)
                    {
                        Debug.WriteLine(localRoute.ResourceSets[0].Resources[0].RouteLegs[0].ItineraryItems[i].Instruction.Text);
                    }
                }
            }
        }
        private static async Task BingMaps(string indirizzo, string postoArancione, string testoparlato, SpeechSynthesizer synthesizer)
        {
            testoparlato = testoparlato.ToLower();
            if (testoparlato.Contains("quanto dista") || testoparlato.Contains("distanza da") || testoparlato.Contains("quanto è distante"))
            {
                await RouteWp1ToWp2("costa masnaga", indirizzo);
            }
            else
            {
                await FindPointOfInterest(indirizzo, postoArancione, 1, synthesizer);
            }
        }
        #endregion

        #region Wikipedia
        static async Task<string> SearchKeyText(string argument)
        {
            string argumentClean = HttpUtility.UrlEncode(argument);
            string wikiUrl = $"https://it.wikipedia.org/w/rest.php/v1/search/page?q={argumentClean}&limit=1";
            // recupero la chiave di ricerca con il parsing del dom
            var response = await _client.GetAsync(wikiUrl);
            if (response.IsSuccessStatusCode)
            {
                Models.KeyModel? model = await response.Content.ReadFromJsonAsync<KeyModel>();
                if (model != null)
                {
                    string? keySearch = model.Pages[0].Key;

                    return keySearch;
                }
            }
            return null;
        }
        static async Task<string> ExtractSummaryByKey(string keySearch)
        {
            string wikiUrl = $"https://it.wikipedia.org/w/api.php?format=json&action=query&prop=extracts&exintro&explaintext&exsectionformat=plain&redirects=1&titles={keySearch}";
            string wikiSummaryJSON = await _client.GetStringAsync(wikiUrl);
            using JsonDocument document = JsonDocument.Parse(wikiSummaryJSON);
            JsonElement root = document.RootElement;
            JsonElement query = root.GetProperty("query");
            JsonElement pages = query.GetProperty("pages");
            JsonElement.ObjectEnumerator enumerator = pages.EnumerateObject();
            if (enumerator.MoveNext())
            {
                JsonElement target = enumerator.Current.Value;
                if (target.TryGetProperty("extract", out JsonElement extract))
                {
                    return extract.GetString() ?? string.Empty;
                }
            }
            return string.Empty;
        }
        static async Task SearchSections(string key, string subsearch, SpeechSynthesizer synthesizer)
        {
            string urlSection = $"https://it.wikipedia.org/w/api.php?action=parse&format=json&page={key}&prop=sections&disabletoc=1";
            var response = await _client.GetAsync(urlSection);
            // parso le sezioni e recupero la key e l'indice di sezione
            subsearch = subsearch.ToLower();
            if (response.IsSuccessStatusCode)
            {
                SectionModel? sectionModel = await response.Content.ReadFromJsonAsync<SectionModel>();
                if (sectionModel != null)
                {
                    List<Section> sections = sectionModel.Parse.Sections;
                    foreach (Section section in sections)
                    {
                        if (section.LinkAnchor.ToLower().Contains(subsearch))
                        {
                            await stampaSezioni(section.Index, key, synthesizer);
                        }
                    }
                }
            }
        }
        public static string sezione = "";
        static async Task stampaSezioni(string ricerca, string key, SpeechSynthesizer synthesizer)
        {
            string url = $"https://it.wikipedia.org/w/api.php?action=parse&format=json&page={key}&prop=wikitext&section={ricerca}&disabletoc=1";
            var response = await _client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                SectionsTexts? sectionText = await response.Content.ReadFromJsonAsync<SectionsTexts>();
                if (sectionText != null)
                {
                    sezione = $"{sectionText.Parses.Wikitext.text}";
                    char[] caratteri = sezione.ToCharArray();
                    string risposta = "";
                    foreach (var n in caratteri)
                    {
                        if (n != '=' && n != '*' && n != '{' && n != '}' && n != '[' && n != ']' && n != '|' && n != '\'')
                        {
                            risposta += n;
                        }
                    }
                    Debug.WriteLine(sezione);
                    sezione = risposta;
                    await synthesizer.SpeakTextAsync(sezione);
                }
            }
        }
        private static async Task WikiIntent(string mainArgument, string subArgument, SpeechSynthesizer synthesizer)
        {
            // cerco la chiave dell'argomento specifico limitato a una risposta
            string? key = await SearchKeyText($"{mainArgument}");
            if (key != null)
            {
                string summary = await ExtractSummaryByKey(key);
                if (subArgument == null || subArgument == "")
                {
                    await synthesizer.SpeakTextAsync($"{summary}");
                }
                else
                {
                    Debug.WriteLine("______________________________________________________");
                    await SearchSections(key, subArgument, synthesizer);
                }
            }
        }
        #endregion
    }
}