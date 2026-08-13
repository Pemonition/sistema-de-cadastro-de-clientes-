using Microsoft.Data.Sqlite;

namespace SistemaCadastroClientes.Infrastructure.Persistence;

// Garante que a tabela exista antes do primeiro uso. Em um projeto maior
// isso seria uma migration; aqui, um CREATE TABLE IF NOT EXISTS resolve bem
// e mantém o script em database/schema.sql como fonte de verdade legível.
public static class DatabaseInitializer
{
    public static void Inicializar(string connectionString)
    {
        using var conexao = new SqliteConnection(connectionString);
        conexao.Open();

        var comando = conexao.CreateCommand();
        comando.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Clientes (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT NOT NULL,
                Cpf TEXT NOT NULL UNIQUE,
                Cep TEXT NOT NULL,
                Logradouro TEXT NOT NULL,
                Numero TEXT NOT NULL,
                Complemento TEXT,
                Bairro TEXT NOT NULL,
                Cidade TEXT NOT NULL,
                Estado TEXT NOT NULL
            );
            """;
        comando.ExecuteNonQuery();
    }
}
