using SistemaCadastroClientes.Domain.Interfaces;
using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Application.Services;

public class ClienteConsultaService
{
    private readonly IClienteRepository _repositorio;

    public ClienteConsultaService(IClienteRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public IReadOnlyList<Cliente> ListarTodos() => _repositorio.ListarTodos();
}
