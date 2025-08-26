using System.Text.Json.Serialization;
using CodiceFiscaleRazorApp.Data;
using CodiceFiscaleRazorApp.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages();


// Estrai da configurazione il provider per l'estrazione dei codici catastali dei comuni
var provider = builder.Configuration["ComuneRepository:Provider"];
if (provider == null)
{
    throw new Exception("ComuneRepository:Provider non impostato in appsettings.json");
}


// Imposta repository per l'estrazione info dei comuni. (Vedi impostazioni "Provider" e "JsonPath" a "ComuneRepository")
builder.Services.AddScoped<IComuneRepository>(sp =>
{
    switch (provider)
    {
        case "Sql":
            var cs = builder.Configuration.GetConnectionString("DefaultConnectionString")
                 ?? throw new InvalidOperationException("Configurazione 'DefaultConnectionString' mancante.");
            return new SqlComuneRepository(cs);
        case "Json":
            return new JsonComuneRepository();
        default:
            throw new Exception($"ComuneRepository:Provider '{provider}' non supportato");
    }
});



// Aggiungi servizi singleton
builder.Services.AddScoped<CodiceFiscaleService>();


// Avvia inizializzatore database se impostato "Sql" come provider dei comuni
if (provider == "Sql")
{
    var connString = builder.Configuration.GetConnectionString("DefaultConnectionString");
    if (connString == null)
    {
        throw new Exception("Connection string mancante");
    }
    var comuniPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data/comuni.json");
    var dbInit = new InizializzatoreDatabaseRepository(connString, comuniPath);
    dbInit.Inizializza();
}



builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}



app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();





// Imposta chiamata minimal-API per il fetch JS  
app.MapPost("/api/codice-fiscale", (CodiceFiscaleRequest req, CodiceFiscaleService svc) =>
{
    // Basic validation (keep it simple here)
    if (req is null || string.IsNullOrWhiteSpace(req.Nome) || string.IsNullOrWhiteSpace(req.Cognome)
        || string.IsNullOrWhiteSpace(req.Comune) || req.DataNascita is null)
        return Results.BadRequest(new { error = "Dati mancanti o non validi" });

    try
    {
        var cf = svc.GetCodiceFiscale(req.Cognome!, req.Nome!, req.DataNascita.Value, req.Comune!, req.Sesso);
        return Results.Ok(new { codiceFiscale = cf });
    }
    catch (CodiceFiscaleException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch(Exception e)
    {
        return Results.InternalServerError();
    }

});



app.Run();



// Record utilizzato da chiamata minimal-api
public record CodiceFiscaleRequest
{
    public string? Cognome { get; set; }
    public string? Nome { get; set; }
    public DateOnly? DataNascita { get; set; } // "yyyy-MM-dd"
    public string? Comune { get; set; }
    public CodiceFiscaleService.Sesso Sesso { get; set; }
}