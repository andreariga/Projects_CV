using PokeAPI.Data;
using PokeAPI.Services;
using PokeAPI.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<ITMDBHttpClientService, TMDBHttpClientService>();
builder.Services.AddSingleton<IRequestValidationService, RequestValidationService>();


builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
	options.AddFixedWindowLimiter("TMDBPolicy", configure =>
	{
		configure.PermitLimit = 100;
		configure.Window = TimeSpan.FromMinutes(1);
		configure.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
		configure.QueueLimit = 2;
	});
});


builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApiDocument(config =>
	{
		config.Title = "PokeAPI";
		config.DocumentName = "Poke API";
		config.Version = "v1";
	}
);
if (builder.Environment.IsDevelopment())
{
	builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

var connectionString = builder.Configuration.GetConnectionString("PokeAPIConnection");
var serverVersion = ServerVersion.AutoDetect(connectionString);

builder.Services.AddDbContext<PokeDbContext>(
	opt => opt.UseMySql(connectionString, serverVersion)
	.LogTo(Console.WriteLine, LogLevel.Information)
	.EnableSensitiveDataLogging()
	.EnableDetailedErrors()
	);

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseOpenApi();
	
	app.UseSwaggerUi(config =>
	{
		config.DocumentTitle = "Poke API ";
		config.Path = "/swagger";
		config.DocumentPath = "/swagger/{documentName}/swagger.json";
		config.DocExpansion = "list";
	});
}
//altri middleware
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
app.UseHttpsRedirection();
// Configure Swagger UI before static files
app.UseSwaggerUi(config =>
{
	config.DocumentTitle = "Poke API ";
	config.Path = "/swagger";
	config.DocumentPath = "/swagger/{documentName}/swagger.json";
	config.DocExpansion = "list";
});

// Serve static files after Swagger

// Middleware per file statici

// 1. Configura il middleware per servire index.html dalla cartella pages alla root
app.UseDefaultFiles(new DefaultFilesOptions
{
	FileProvider = new PhysicalFileProvider(
		Path.Combine(builder.Environment.WebRootPath, "pages")
	),
	RequestPath = ""
});

// 2. Middleware per file statici in wwwroot (CSS, JS, ecc.)
app.UseStaticFiles();

// 3. Middleware di fallback per cercare file in wwwroot/pages
app.UseStaticFiles(new StaticFileOptions
{
	FileProvider = new PhysicalFileProvider(
		Path.Combine(builder.Environment.WebRootPath, "pages")
	)
});

app.UseRateLimiter();
// routing per le API
//--------------------Endpoints management--------------------
app
.MapGroup("/api")
.MapUtentiEndpoints()
.MapDatasEndpoints()
.MapAttackEndpoints()
.MapWeaknessEndpoints()
.MapHolofoilEndpoints()
.MapImagesEndpoints()
.MapLegalitiesEndpoints()
.MapPricesEndpoints()
.MapSetEndpoints()
.MapTcgplayerEndpoints()
.MapTMDBProxyEndpoints()
.MapGetCardsEnpoints()
.WithOpenApi()
.WithTags("Public API");

//--------------------Endpoints management--------------------

app.Run();