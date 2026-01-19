using System.Text.Json.Serialization;

namespace AliceNeural.Models
{
    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    public class Parses
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("pageid")]
        public int Pageid { get; set; }

        [JsonPropertyName("wikitext")]
        public Wikitext Wikitext { get; set; }
    }

    public class SectionsTexts
    {
        [JsonPropertyName("parse")]
        public Parses Parses { get; set; }
    }

    public class Wikitext
    {
        [JsonPropertyName("*")]
        public string text { get; set; }

    }

}