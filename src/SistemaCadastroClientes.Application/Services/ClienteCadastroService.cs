using SistemaCadastroClientes.Application.Validation;
using SistemaCadastroClientes.Domain.Exceptions;
using SistemaCadastroClientes.Domain.Interfaces;
using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Application.Services;

// Concentra as regras de negócio do cadastro (RN01-RN09). Repare que esta
// classe conhece IClienteRepository (uma abstração), nunca SqliteClienteRepository.
// Ela não sabe nada sobre CEP/HTTP: quem monta o Endereco antes de chamar
// Cadastrar() é a camada de UI, usando o CepConsultaService.
public class ClienteCadastroService
{
    private readonly IClienteRepository _repositorio;

    public ClienteCadastroService(IClienteRepository repositorio)
    {
        _repositorio = repositorio;
    }

    public void Cadastrar(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nome)) // RN01
            throw new DadosObrigatoriosAusentesException("Nome");

        if (string.IsNullOrWhiteSpace(cliente.Cpf)) // RN02
            throw new DadosObrigatoriosAusentesException("CPF");

        cliente.Cpf = CpfValidator.Normalizar(cliente.Cpf);

        if (!CpfValidator.EhValido(cliente.Cpf)) // RN03
            throw new CpfInvalidoException(cliente.Cpf);

        if (string.IsNullOrWhiteSpace(cliente.Endereco.Numero)) // RN08
            throw new DadosObrigatoriosAusentesException("Número do endereço");

        // ExisteClienteComCpf e Adicionar já traduzem falhas de banco para
        // PersistenciaException dentro do repositório (RN09) — esta classe
        // não precisa saber que existe um banco de dados por trás.
        if (_repositorio.ExisteClienteComCpf(cliente.Cpf)) // RN04
            throw new CpfDuplicadoException(cliente.Cpf);

        _repositorio.Adicionar(cliente);
    }
}
