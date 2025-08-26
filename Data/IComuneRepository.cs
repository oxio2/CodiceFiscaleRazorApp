namespace CodiceFiscaleRazorApp.Data;

public interface IComuneRepository
{
    string? GetCodiceCatastale(string comune);
}
