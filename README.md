# CodiceFiscaleRazorApp

Form di inserimento dati anagrafici per la generazione del codice fiscale

## Funzionalità

L'applicazione permette di eseguire il calcolo del codice fiscale italiano in due modalità:

- SSR(Server Side Rendering): Viene utilizzato un normale ciclo di richiesta/risposta HTTP
- SPA(Single Page Application): Viene utilizzato javascript per inviare una richiesta fetch ad un endpoint minimal api e aggiornare dinamicamente la pagina senza ricaricarla

## Impostazioni dell'applicazione
- "ConnectionStrings":"DefaultConnectionString" --> Stringa di connessione al database SQL Server
- "ComuneRepository"
	- "Json" --> Utilizza il file json "comuni.json" per l'estrazione dei codici catastali dei comuni 
	- "Sql" --> Utilizza la connessione al database SQL Server per l'estrazione dei codici catastali dei comuni (La tabella "Comuni" viene automaticamente popolata in base ai dati presenti a file json "comuni.json")