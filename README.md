# CodiceFiscaleRazorApp

Semplice applicazione Razor Pages per la generazione del codice fiscale italiano a partire dai dati anagrafici di una persona.

## Requisiti
- .NET 9
- SQL Server (opzionale, solo se impostato `ComuneRepository` a `Sql`. Vedi "Impostazioni dell'applicazione")

## Impostazioni dell'applicazione
- `ConnectionStrings`:`DefaultConnectionString` --> Stringa di connessione al database SQL Server
- `ComuneRepository`
	- `Json` --> Utilizza il file json `comuni.json` per l'estrazione dei codici catastali dei comuni 
	- `Sql` --> Utilizza la connessione al database SQL Server per l'estrazione dei codici catastali dei comuni (La tabella `Comuni` viene automaticamente popolata in base ai dati presenti a file json `comuni.json`)