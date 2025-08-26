using Microsoft.Data.SqlClient;

namespace CodiceFiscaleRazorApp.Data;

public class SqlComuneRepository : IComuneRepository
{
    private readonly string _connString;

    public SqlComuneRepository(string connString)
    {
        _connString = connString;
    }

    public string? GetCodiceCatastale(string comune)
    {
        using var conn = new SqlConnection(_connString);
        conn.Open();

        var cmd = new SqlCommand(
            "SELECT CodiceCatastale FROM Comuni WHERE Nome = @nome",
            conn);
        cmd.Parameters.AddWithValue("@nome", comune);

        return cmd.ExecuteScalar() as string;
    }
}
