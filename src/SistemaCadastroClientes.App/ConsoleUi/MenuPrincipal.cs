using SistemaCadastroClientes.Application.Services;
using SistemaCadastroClientes.Domain.Exceptions;
using SistemaCadastroClientes.Domain.Models;
using SistemaCadastroClientes.Infrastructure.CepProviders;

namespace SistemaCadastroClientes.App.ConsoleUi;

// Única responsabilidade desta classe: interação com o usuário (ler/exibir
// texto no console). Toda regra de negócio fica nos serviços da camada
// Application; toda decisão de "qual provedor/banco usar" já foi resolvida
// no composition root (Program.cs) antes de chegar aqui.
public class MenuPrincipal
{
    private readonly ClienteCadastroService _cadastroService;
    private readonly ClienteConsultaService _consultaService;
    private readonly CepConsultaService _cepService;
    private readonly CepProviderFactory _providerFactory;

    public MenuPrincipal(
        ClienteCadastroService cadastroService,
        ClienteConsultaService consultaService,
        CepConsultaService cepService,
        CepProviderFactory providerFactory)
    {
        _cadastroService = cadastroService;
        _consultaService = consultaService;
        _cepService = cepService;
        _providerFactory = providerFactory;
    }

    public async Task ExecutarAsync()
    {
        Console.WriteLine("=== Sistema de Cadastro de Clientes — Ardena Soluções ===");
        EscolherProvedor();

        var continuar = true;
        while (continuar)
        {
            Console.WriteLine();
            Console.WriteLine($"Provedor de CEP atual: {_cepService.ProvedorAtual}");
            Console.WriteLine("1) Cadastrar cliente");
            Console.WriteLine("2) Consultar clientes cadastrados");
            Console.WriteLine("3) Trocar provedor de CEP");
            Console.WriteLine("0) Sair");
            Console.Write("Escolha uma opção: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                    await CadastrarClienteAsync();
                    PausarAntesDeContinuar();
                    break;
                case "2":
                    ConsultarClientes();
                    PausarAntesDeContinuar();
                    break;
                case "3":
                    EscolherProvedor();
                    PausarAntesDeContinuar();
                    break;
                case "0":
                    continuar = false;
                    break;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
    }

    private void EscolherProvedor()
    {
        var provedores = _providerFactory.Listar();
        Console.WriteLine();
        Console.WriteLine("Provedores de CEP disponíveis:");
        for (var i = 0; i < provedores.Count; i++)
            Console.WriteLine($"{i + 1}) {provedores[i].Nome}");

        Console.Write("Escolha o provedor: ");
        if (int.TryParse(Console.ReadLine(), out var opcao) && opcao >= 1 && opcao <= provedores.Count)
        {
            _cepService.TrocarProvedor(_providerFactory.ObterPorIndice(opcao - 1));
            Console.WriteLine($"Provedor selecionado: {_cepService.ProvedorAtual}");
        }
        else
        {
            Console.WriteLine("Opção inválida. Mantendo o provedor atual.");
        }
    }

    private async Task CadastrarClienteAsync()
    {
        Console.WriteLine();
        Console.WriteLine("--- Cadastro de cliente ---");

        Console.Write("Nome completo: ");
        var nome = Console.ReadLine() ?? string.Empty;

        Console.Write("CPF: ");
        var cpf = Console.ReadLine() ?? string.Empty;

        Endereco? endereco = null;
        while (endereco is null)
        {
            Console.Write("CEP: ");
            var cep = Console.ReadLine() ?? string.Empty;

            ResultadoConsultaCep resultado;
            try
            {
                resultado = await _cepService.ConsultarAsync(cep);
            }
            catch (ServicoIndisponivelException ex)
            {
                Console.WriteLine($"Não foi possível consultar o CEP agora: {ex.Message}");
                if (!PerguntarSimNao("Tentar novamente com outro CEP? (s/n): ")) return;
                continue;
            }
            catch (DadosObrigatoriosAusentesException ex)
            {
                Console.WriteLine(ex.Message);
                continue;
            }

            if (!resultado.Encontrado)
            {
                Console.WriteLine("CEP não encontrado.");
                if (!PerguntarSimNao("Tentar novamente com outro CEP? (s/n): ")) return;
                continue;
            }

            endereco = resultado.Endereco;
            Console.WriteLine($"Endereço encontrado: {endereco!.Logradouro}, {endereco.Bairro}, {endereco.Cidade}/{endereco.Estado}");
        }

        Console.Write("Número: ");
        endereco.Numero = Console.ReadLine() ?? string.Empty;

        Console.Write("Complemento (opcional): ");
        var complemento = Console.ReadLine();
        endereco.Complemento = string.IsNullOrWhiteSpace(complemento) ? null : complemento;

        Console.WriteLine();
        Console.WriteLine("--- Confirme os dados ---");
        Console.WriteLine($"Nome: {nome}");
        Console.WriteLine($"CPF: {cpf}");
        Console.WriteLine($"Endereço: {endereco.Logradouro}, {endereco.Numero} {endereco.Complemento} - {endereco.Bairro}, {endereco.Cidade}/{endereco.Estado} - {endereco.Cep}");

        if (!PerguntarSimNao("Confirmar cadastro? (s/n): "))
        {
            Console.WriteLine("Cadastro cancelado.");
            return;
        }

        var cliente = new Cliente { Nome = nome, Cpf = cpf, Endereco = endereco };

        try
        {
            _cadastroService.Cadastrar(cliente);
            Console.WriteLine($"Cliente cadastrado com sucesso (Id {cliente.Id}).");
        }
        catch (CpfInvalidoException ex) { Console.WriteLine(ex.Message); }
        catch (CpfDuplicadoException ex) { Console.WriteLine(ex.Message); }
        catch (DadosObrigatoriosAusentesException ex) { Console.WriteLine(ex.Message); }
        catch (PersistenciaException ex) { Console.WriteLine(ex.Message); }
    }

    private void ConsultarClientes()
    {
        Console.WriteLine();
        Console.WriteLine("--- Clientes cadastrados ---");

        IReadOnlyList<Cliente> clientes;
        try
        {
            clientes = _consultaService.ListarTodos();
        }
        catch (PersistenciaException ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }

        if (clientes.Count == 0)
        {
            Console.WriteLine("Nenhum cliente cadastrado.");
            return;
        }

        foreach (var cliente in clientes)
        {
            Console.WriteLine(
                $"[{cliente.Id}] {cliente.Nome} - CPF {cliente.Cpf} - " +
                $"{cliente.Endereco.Logradouro}, {cliente.Endereco.Numero} - {cliente.Endereco.Bairro}, " +
                $"{cliente.Endereco.Cidade}/{cliente.Endereco.Estado} - CEP {cliente.Endereco.Cep}");
        }
    }

    // Sem isso, o resultado de um cadastro/consulta some da tela assim que
    // o menu é redesenhado na próxima volta do loop — dá a impressão de
    // que "nada aconteceu", mesmo com a mensagem certa tendo sido impressa.
    private static void PausarAntesDeContinuar()
    {
        Console.WriteLine();
        Console.Write("Pressione Enter para voltar ao menu...");
        Console.ReadLine();
    }

    private static bool PerguntarSimNao(string pergunta)
    {
        Console.Write(pergunta);
        var resposta = Console.ReadLine()?.Trim().ToLowerInvariant();
        return resposta == "s" || resposta == "sim";
    }
}
