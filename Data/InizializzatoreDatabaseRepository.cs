
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CodiceFiscaleRazorApp.Data;

public class InizializzatoreDatabaseRepository
{
    protected string _connString;
    protected string _comuniJsonPath;


    // Modello JSON
    public record Comune(
        string? nome,
        string? codice,
        Zona? zona,
        Regione? regione,
        Provincia? provincia,
        string? sigla,
        string? codiceCatastale,
        string[]? cap,
        int? popolazione
    );

    public record Zona(string? codice, string? nome);
    public record Regione(string? codice, string? nome);
    public record Provincia(string? codice, string? nome);


    public InizializzatoreDatabaseRepository(string connString, string comuniJsonPath)
    {
        _connString = connString;
        _comuniJsonPath = comuniJsonPath;
    }

    public void Inizializza()
    {
        InizializzaTabellaComuni();
        SeedComuni(_comuniJsonPath);
    }

    protected void SeedComuni(string comuniJsonPath)
    {
        // Leggi file JSON
        var json = File.ReadAllText(comuniJsonPath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        var comuni = JsonSerializer.Deserialize<List<Comune>>(json, options) ?? new();


        // Imposta datatable da importare in bulk a database
        var dt = new DataTable();
        dt.Columns.Add("Codice", typeof(string));
        dt.Columns.Add("Nome", typeof(string));
        dt.Columns.Add("ZonaCodice", typeof(string));
        dt.Columns.Add("ZonaNome", typeof(string));
        dt.Columns.Add("RegioneCodice", typeof(string));
        dt.Columns.Add("RegioneNome", typeof(string));
        dt.Columns.Add("ProvinciaCodice", typeof(string));
        dt.Columns.Add("ProvinciaNome", typeof(string));
        dt.Columns.Add("Sigla", typeof(string));
        dt.Columns.Add("CodiceCatastale", typeof(string));
        dt.Columns.Add("Cap", typeof(string));
        dt.Columns.Add("Popolazione", typeof(int));


        // Aggiungi dati a datatable
        foreach (var c in comuni)
        {
            var cap = c.cap != null && c.cap.Length > 0 ? c.cap[0] : null;
            dt.Rows.Add(
                c.codice,
                c.nome,
                c.zona?.codice,
                c.zona?.nome,
                c.regione?.codice,
                c.regione?.nome,
                c.provincia?.codice,
                c.provincia?.nome,
                c.sigla,
                c.codiceCatastale,
                cap,
                c.popolazione is null ? DBNull.Value : c.popolazione
            );
        }


        // Prepara connessione e transazione a database
        using var conn = new SqlConnection(_connString);
        conn.Open();
        using var tx = conn.BeginTransaction();
        

        try
        {
            // Pulisci tabella
            using (var del = new SqlCommand("DELETE FROM [dbo].[Comuni];", conn, tx))
                del.ExecuteNonQuery();


            // Esegui Bulk Insert
            using (var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, tx))
            {
                bulk.DestinationTableName = "[dbo].[Comuni]";
                foreach (DataColumn col in dt.Columns)
                    bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);

                bulk.WriteToServer(dt);
            }
        }
        catch(Exception ex)
        {
            tx.Rollback();
            Console.Error.WriteLine("Errore: " + ex.Message);
            throw;
        }
       

        tx.Commit();
    }


    protected void InizializzaTabellaComuni()
    {
        using (SqlConnection conn = new SqlConnection(_connString))
        {
            conn.Open();

            string query =
                @"IF NOT EXISTS (
                    SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Comuni]') AND type = N'U'
                )
                BEGIN
                    CREATE TABLE [dbo].[Comuni](
                    [Codice]           nvarchar(6)   NOT NULL PRIMARY KEY, -- ""028001"", ""098001""
                    [Nome]             nvarchar(200) NOT NULL,
                    [ZonaCodice]       nvarchar(10)  NULL,
                    [ZonaNome]         nvarchar(50)  NULL,
                    [RegioneCodice]    nvarchar(10)  NULL,
                    [RegioneNome]      nvarchar(50)  NULL,
                    [ProvinciaCodice]  nvarchar(10)  NULL,
                    [ProvinciaNome]    nvarchar(100) NULL,
                    [Sigla]            nvarchar(4)   NULL,
                    [CodiceCatastale]  nvarchar(10)  NULL,
                    [Cap]              nvarchar(10)  NULL,  -- just first CAP (or duplicate if multiple)
                    [Popolazione]      int           NULL
                    );
                END;
                ";
            
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
       
    }
}
