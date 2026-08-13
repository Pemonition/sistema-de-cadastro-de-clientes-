using MySqlConnector;

namespace SistemaCadastroClientes.Infrastructure.Persistence;

// Equivalente ao DatabaseInitializer, mas para MySQL. A sintaxe de DDL
// muda pouco entre os dois bancos (AUTO_INCREMENT em vez de AUTOINCREMENT,
// tipos com tamanho explícito), mas ainda assim é código diferente — daí
// existir uma classe própria em vez de tentar compartilhar SQL entre os
// dois provedores de banco.
public static class MySqlDatabaseInitializer
{
    public static void Inicializar(string connectionString)
    {
        using var conexao = new MySqlConnection(connectionString);
        conexao.Open();

        var comando = conexao.CreateCommand();
        comando.CommandText =
            """
            CREATE TABLE IF NOT EXISTS Clientes (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Nome VARCHAR(200) NOT NULL,
                Cpf VARCHAR(11) NOT NULL UNIQUE,
                Cep VARCHAR(8) NOT NULL,
                Logradouro VARCHAR(200) NOT NULL,
                Numero VARCHAR(20) NOT NULL,
                Complemento VARCHAR(100) NULL,
                Bairro VARCHAR(100) NOT NULL,
                Cidade VARCHAR(100) NOT NULL,
                Estado CHAR(2) NOT NULL
            );
            """;
        comando.ExecuteNonQuery();
    }
}
