using SistemaCadastroClientes.Domain.Interfaces;
using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Application.Services;

// Este é o "Contexto" clássico do padrão Strategy: guarda uma referência ao
// ICepProvider escolhido pelo usuário (RN10) e delega a consulta para ele,
// sem saber (nem precisar saber) qual provedor concreto está por trás.
// Trocar de provedor em tempo de execução é só reatribuir _provedorAtual.
public class CepConsultaService
{
    private ICepProvider _provedorAtual;

    public CepConsultaService(ICepProvider provedorInicial)
    {
        _provedorAtual = provedorInicial;
    }

    public string ProvedorAtual => _provedorAtual.Nome;

    public void TrocarProvedor(ICepProvider provedor)
    {
        _provedorAtual = provedor;
    }

    public Task<ResultadoConsultaCep> ConsultarAsync(string cep)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new Domain.Exceptions.DadosObrigatoriosAusentesException("CEP");

        return _provedorAtual.ConsultarAsync(cep);
    }
}
