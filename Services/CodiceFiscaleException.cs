namespace CodiceFiscaleRazorApp.Services;

public class CodiceFiscaleException : Exception
{
    public CodiceFiscaleException(string message) : base(message) { }
    public CodiceFiscaleException(string message, Exception inner) : base(message, inner) { }
}