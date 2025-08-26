using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CodiceFiscaleRazorApp.Services;

namespace CodiceFiscaleRazorApp.Pages;

public class CodiceFiscaleModel : PageModel
{
    private readonly CodiceFiscaleService _svc;
    public CodiceFiscaleModel(CodiceFiscaleService svc) => _svc = svc;

    [BindProperty, Required] public string Cognome { get; set; } = "";
    [BindProperty, Required] public string Nome { get; set; } = "";
    [BindProperty, Required, DataType(DataType.Date)] public DateOnly? DataNascita { get; set; }
    [BindProperty, Required] public string Comune { get; set; } = "";
    [BindProperty, Required] public CodiceFiscaleService.Sesso Sesso { get; set; }

    public string? CodiceFiscale { get; private set; }
    public string? Errore { get; private set; }

    public void OnGet() { }

    public void OnPost()
    {
        if (!ModelState.IsValid || DataNascita is null) return;
        try
        {
            CodiceFiscale = _svc.GetCodiceFiscale(Cognome, Nome, DataNascita.Value, Comune, Sesso);
        }
        catch (CodiceFiscaleException ex)
        {
            Errore = ex.Message;
        }
        catch(Exception e)
        {
            Errore = "Errore di sistema. Riprova più tardi";
        }
    }
}
