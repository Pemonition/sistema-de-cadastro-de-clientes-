using SistemaCadastroClientes.Domain.Interfaces;

namespace SistemaCadastroClientes.Infrastructure.CepProviders;

// FACTORY: sabe quais estratégias (ICepProvider) existem e resolve uma
// delas por nome ou índice. Para adicionar um terceiro provedor, basta
// criar a classe e incluí-la na lista montada no composition root
// (Program.cs) — nenhuma outra parte do sistema muda (RN12, Desafio Final).
public class CepProviderFactory
{
    private readonly List<ICepProvider> _provedores;

    public CepProviderFactory(IEnumerable<ICepProvider> provedoresDisponiveis)
    {
        _provedores = provedoresDisponiveis.ToList();
        if (_provedores.Count == 0)
            throw new InvalidOperationException("Nenhum provedor de CEP foi registrado.");
    }

    public IReadOnlyList<ICepProvider> Listar() => _provedores;

    public ICepProvider ObterPorIndice(int indice)
    {
        if (indice < 0 || indice >= _provedores.Count)
            throw new InvalidOperationException("Índice de provedor inválido.");

        return _provedores[indice];
    }
}
