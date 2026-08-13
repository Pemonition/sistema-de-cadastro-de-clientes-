using Microsoft.Data.Sqlite;
using SistemaCadastroClientes.Domain.Exceptions;
using SistemaCadastroClientes.Domain.Interfaces;
using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Infrastructure.Persistence;

// REPOSITORY concreto: única classe do sistema que sabe escrever SQL.
// Usa ADO.NET puro (Microsoft.Data.Sqlite) em vez de um ORM para deixar
// explícito, para fins didáticos, o que o Repository esconde do resto da
// aplicação.
public class SqliteClienteRepository : IClienteRepository
{
    private readonly string _connectionString;

    public SqliteClienteRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Adicionar(Cliente cliente)
    {
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            var comando = conexao.CreateCommand();
            comando.CommandText =
                """
                INSERT INTO Clientes (Nome, Cpf, Cep, Logradouro, Numero, Complemento, Bairro, Cidade, Estado)
                VALUES ($nome, $cpf, $cep, $logradouro, $numero, $complemento, $bairro, $cidade, $estado);
                SELECT last_insert_rowid();
                """;
            comando.Parameters.AddWithValue("$nome", cliente.Nome);
            comando.Parameters.AddWithValue("$cpf", cliente.Cpf);
            comando.Parameters.AddWithValue("$cep", cliente.Endereco.Cep);
            comando.Parameters.AddWithValue("$logradouro", cliente.Endereco.Logradouro);
            comando.Parameters.AddWithValue("$numero", cliente.Endereco.Numero);
            comando.Parameters.AddWithValue("$complemento", (object?)cliente.Endereco.Complemento ?? DBNull.Value);
            comando.Parameters.AddWithValue("$bairro", cliente.Endereco.Bairro);
            comando.Parameters.AddWithValue("$cidade", cliente.Endereco.Cidade);
            comando.Parameters.AddWithValue("$estado", cliente.Endereco.Estado);

            cliente.Id = Convert.ToInt32((long)comando.ExecuteScalar()!);
        }
        catch (SqliteException ex)
        {
            throw new PersistenciaException("Não foi possível salvar o cliente no banco de dados.", ex);
        }
    }

    public bool ExisteClienteComCpf(string cpf)
    {
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            var comando = conexao.CreateCommand();
            comando.CommandText = "SELECT COUNT(1) FROM Clientes WHERE Cpf = $cpf;";
            comando.Parameters.AddWithValue("$cpf", cpf);

            var quantidade = (long)comando.ExecuteScalar()!;
            return quantidade > 0;
        }
        catch (SqliteException ex)
        {
            throw new PersistenciaException("Não foi possível consultar o CPF no banco de dados.", ex);
        }
    }

    public IReadOnlyList<Cliente> ListarTodos()
    {
        try
        {
            using var conexao = new SqliteConnection(_connectionString);
            conexao.Open();

            var comando = conexao.CreateCommand();
            comando.CommandText =
                "SELECT Id, Nome, Cpf, Cep, Logradouro, Numero, Complemento, Bairro, Cidade, Estado FROM Clientes ORDER BY Nome;";

            var clientes = new List<Cliente>();
            using var leitor = comando.ExecuteReader();
            while (leitor.Read())
            {
                clientes.Add(new Cliente
                {
                    Id = leitor.GetInt32(0),
                    Nome = leitor.GetString(1),
                    Cpf = leitor.GetString(2),
                    Endereco = new Endereco
                    {
                        Cep = leitor.GetString(3),
                        Logradouro = leitor.GetString(4),
                        Numero = leitor.GetString(5),
                        Complemento = leitor.IsDBNull(6) ? null : leitor.GetString(6),
                        Bairro = leitor.GetString(7),
                        Cidade = leitor.GetString(8),
                        Estado = leitor.GetString(9),
                    },
                });
            }

            return clientes;
        }
        catch (SqliteException ex)
        {
            throw new PersistenciaException("Não foi possível consultar os clientes no banco de dados.", ex);
        }
    }
}
