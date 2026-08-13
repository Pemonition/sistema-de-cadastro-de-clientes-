using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Domain.Interfaces;

// REPOSITORY: isola a lógica de cadastro de qualquer detalhe de persistência
// (SQL, ADO.NET, o provedor de banco escolhido). Se amanhã trocarmos SQLite
// por outro banco, só a implementação em Infrastructure muda.
public interface IClienteRepository
{
    void Adicionar(Cliente cliente);

    bool ExisteClienteComCpf(string cpf);

    IReadOnlyList<Cliente> ListarTodos();
}
