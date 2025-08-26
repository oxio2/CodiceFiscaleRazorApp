using System.Text.Json;

namespace CodiceFiscaleRazorApp.Data;

public class JsonComuneRepository : IComuneRepository
{
    private readonly string _filePath;

    public JsonComuneRepository()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "App_Data", "comuni.json");
    }

    /**
     * Restituisce il codice catastale del comune leggendo da un file JSON
     * Il file JSON deve essere nella cartella App_Data e deve avere la seguente struttura:
     * [
     *   {
     *     "nome": "Roma",
     *     "codiceCatastale": "H501"
     *   },
     *   {
     *     "nome": "Milano",
     *     "codiceCatastale": "F205"
     *   }
     * ]
     */
    public string? GetCodiceCatastale(string comune)
    {
        if (!File.Exists(_filePath)) return null;

        var json = File.ReadAllText(_filePath);
        using var doc = JsonDocument.Parse(json);

        foreach (var comuneObj in doc.RootElement.EnumerateArray())
        {
            if (comuneObj.TryGetProperty("nome", out var nomeProp) &&
                comuneObj.TryGetProperty("codiceCatastale", out var codiceProp))
            {
                var nome = nomeProp.GetString();
                var codiceCatastale = codiceProp.GetString();

                if (!string.IsNullOrEmpty(nome) &&
                    !string.IsNullOrEmpty(codiceCatastale) &&
                    string.Equals(nome, comune, StringComparison.OrdinalIgnoreCase))
                {
                    return codiceCatastale;
                }
            }
        }

        return null;
    }
}
