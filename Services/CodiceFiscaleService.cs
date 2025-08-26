using System.IO.Pipelines;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using CodiceFiscaleRazorApp.Data;

namespace CodiceFiscaleRazorApp.Services;

public class CodiceFiscaleService
{
    readonly protected IComuneRepository _comuneRepository;

    public CodiceFiscaleService(IComuneRepository comuneRepository)
    {
        this._comuneRepository = comuneRepository;
    }

    public enum Sesso
    {
        Maschio,
        Femmina
    }

    /**
     * Restituisce il codice fiscale completo
     */
    public string? GetCodiceFiscale(string cognome, string nome, DateOnly dataNascita, string comune, Sesso sesso)
    {
        string codiceFiscale = "";

        codiceFiscale += GetCodiceCognome(cognome);
        codiceFiscale += GetCodiceNome(nome);
        codiceFiscale += GetCodiceAnnoNascita(dataNascita);
        codiceFiscale += GetCodiceMeseNascita(dataNascita);
        codiceFiscale += GetCodiceGiornoNascita(dataNascita, sesso);
        codiceFiscale += GetCodiceComune(comune);
        codiceFiscale += GetCodiceControllo(codiceFiscale);

        return codiceFiscale;
    }

    /**
     * Restituisce il carattere di controllo del codice fiscale
     */
    protected string GetCodiceControllo(string codiceFiscaleParziale)
    {
        var (cinPari, cinDispari) = GetCodiciCinJson();
        int somma = 0;
        for (int i = 0; i < codiceFiscaleParziale.Length; i++)
        {
            char c = codiceFiscaleParziale[i];
            if ((i + 1) % 2 == 0)
            {
                somma += cinPari[c];
            }
            else
            {
                somma += cinDispari[c];
            }
        }
        int resto = somma % 26;
        char carattereControllo = (char)('A' + resto);
        return carattereControllo.ToString();
    }

    /** 
     * Restituisce il codice catastale del comune
     */
    protected string GetCodiceComune(string comune)
    {
        string? codiceCatastale = this._comuneRepository.GetCodiceCatastale(comune);
        if (codiceCatastale == null)
        {
            throw new CodiceFiscaleException("Comune non trovato");
        }

        return codiceCatastale;
    }


    /**     * Restituisce i codici CIN per il calcolo del carattere di controllo leggendo da un file JSON
     * Il file JSON deve essere nella cartella App_Data e deve avere la seguente struttura:
     * {
     *   "pari": {
     *     "A": 0,
     *     "B": 1,
     *     ...
     *   },
     *   "dispari": {
     *     "A": 1,
     *     "B": 0,
     *     ...
     *   }
     * }
     */
    protected (Dictionary<char, int> CinPari, Dictionary<char, int> CinDispari) GetCodiciCinJson()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "App_Data", "cin.json");
        var json = File.ReadAllText(filePath);

        using var doc = JsonDocument.Parse(json);

        Dictionary<char, int> ToDict(JsonElement element)
        {
            var dict = new Dictionary<char, int>();
            foreach (var kvp in element.EnumerateObject())
            {
                dict[char.ToUpperInvariant(kvp.Name[0])] = kvp.Value.GetInt32();
            }
            return dict;
        }

        var pari = ToDict(doc.RootElement.GetProperty("pari"));
        var dispari = ToDict(doc.RootElement.GetProperty("dispari"));

        return (pari, dispari);
    }

    



    /**
     * Restituisce le ultime due cifre dell'anno di nascita
     */
    protected string GetCodiceAnnoNascita(DateOnly dataNascita)
    {
        return dataNascita.ToString("yy");
    }

    /**
     * Restituisce una lettera per il mese di nascita
     * (A = Gennaio, B = Febbraio, C = Marzo, D = Aprile, E = Maggio, H = Giugno, L = Luglio, M = Agosto, P = Settembre, R = Ottobre, S = Novembre, T = Dicembre)
     */
    protected string GetCodiceMeseNascita(DateOnly dataNascita)
    {
        char[] mesiCodici = { 'A', 'B', 'C', 'D', 'E', 'H', 'L', 'M', 'P', 'R', 'S', 'T' };
        int meseIndex = dataNascita.Month - 1; // DateOnly.Month is 1-based
        return mesiCodici[meseIndex].ToString();
    }

    /**
     * Restituisce il giorno di nascita, aggiungendo 40 se il sesso è femmina
     */
    protected string GetCodiceGiornoNascita(DateOnly dataNascita, Sesso sesso)
    {
        int giorno = dataNascita.Day;
        if(sesso == Sesso.Femmina)
        {
            giorno += 40;
        }

        return giorno.ToString().PadLeft(2, '0');
    }



    /**
     * Restituisce le prime tre lettere del cognome
     * Se il cognome ha meno di tre lettere, aggiunge delle X
     */
    protected string GetCodiceCognome(string cognome)
    {
        return GetCodiceNomeCognome(cognome, false);
    }


    /**
     * Restituisce le prime tre consonanti del nome
     * Se il nome ha meno di tre consonanti, aggiunge delle X
     * Se il nome ha più di tre consonanti, prende la prima, la terza e la quarta
     */
    protected string GetCodiceNome(string nome)
    {
        return GetCodiceNomeCognome(nome, true);
    }


    /**     
     * Restituisce le prime tre lettere del nome o cognome
     * Se il nome o cognome ha meno di tre lettere, aggiunge delle X
     * Se il nome ha più di tre consonanti e applicaRegolaQuattroConsonanti è true, prende la prima, la terza e la quarta consonante
     */
    protected string GetCodiceNomeCognome(string input, bool applicaRegolaQuattroConsonanti)
    {
        string consonantiTrovate = "";
        string consonanti = "BCDFGHJKLMNPQRSTVWXYZ";

        foreach (char c in input)
        {
            if (consonanti.Contains(char.ToUpper(c)))
            {
                consonantiTrovate += char.ToUpper(c);
            }
        }

        if (applicaRegolaQuattroConsonanti && consonantiTrovate.Length >= 4)
        {
            return "" + consonantiTrovate[0] + consonantiTrovate[2] + consonantiTrovate[3];
        }

        if (consonantiTrovate.Length >= 3)
        {
            return new string(consonantiTrovate).Substring(0, 3);
        }


        string vocali = "AEIOU";
        string vocaliTrovate = "";
        foreach (char c in input)
        {
            if (vocali.Contains(char.ToUpper(c)))
            {
                vocaliTrovate += char.ToUpper(c);
            }
        }

        string output = consonantiTrovate + vocaliTrovate;
        output = output.PadRight(3, 'X');
        output = output.Substring(0, 3);
        

        return output;
    }

}