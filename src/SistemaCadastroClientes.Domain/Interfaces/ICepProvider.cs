using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Domain.Interfaces;

// STRATEGY: cada provedor de CEP (ViaCEP, BrasilAPI, um terceiro futuro...)
// implementa esta mesma interface. A camada de aplicação depende só dela —
// nunca de HttpClient, de um endpoint específico ou de um formato de JSON.
// Isso é o que permite trocar de provedor em tempo de execução (RN10) e
// adicionar um novo sem tocar nas regras de cadastro (RN11, RN12).
public interface ICepProvider
{
    // Nome de exibição usado no menu (ex.: "ViaCEP"). É o identificador
    // pelo qual o usuário escolhe a estratégia a ser usada.
    string Nome { get; }

    // Lança ServicoIndisponivelException em caso de falha de comunicação.
    // "CEP não encontrado" NÃO é uma exceção: vem como
    // ResultadoConsultaCep.NaoEncontrado() (RN06).
    Task<ResultadoConsultaCep> ConsultarAsync(string cep);
}
