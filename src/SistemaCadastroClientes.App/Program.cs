using SistemaCadastroClientes.App.ConsoleUi;
using SistemaCadastroClientes.Application.Services;
using SistemaCadastroClientes.Domain.Interfaces;
using SistemaCadastroClientes.Infrastructure.CepProviders;
using SistemaCadastroClientes.Infrastructure.Persistence;

// COMPOSITION ROOT: o único lugar do sistema que conhece as classes
// concretas de todas as camadas ao mesmo tempo (ViaCepProvider,
// SqliteClienteRepository, etc.) e decide como conectá-las. Nenhuma outra
// classe do projeto tem essa visão completa — cada uma só enxerga as
// abstrações (ICepProvider, IClienteRepository) de que precisa.
// Não há container de injeção de dependência aqui de propósito: a ligação
// manual deixa explícito, para fins didáticos, o que um container faria por
// baixo dos panos.

IClienteRepository clienteRepository = EscolherRepositorio();

var httpClient = new HttpClient();

// Cada provedor é uma Strategy concreta. Adicionar um terceiro provedor
// significa criar a classe e incluir uma linha aqui — nenhuma outra parte
// do sistema precisa ser tocada.
var providerFactory = new CepProviderFactory(new ICepProvider[]
{
    new ViaCepProvider(httpClient),
    new BrasilApiProvider(httpClient),
    new OpenCepProvider(httpClient),
});

var cepService = new CepConsultaService(providerFactory.ObterPorIndice(0));
var cadastroService = new ClienteCadastroService(clienteRepository);
var consultaService = new ClienteConsultaService(clienteRepository);

var menu = new MenuPrincipal(cadastroService, consultaService, cepService, providerFactory);
await menu.ExecutarAsync();

// Igual à escolha de provedor de CEP, a escolha do banco também é uma
// troca de implementação por trás de uma única interface
// (IClienteRepository) — prova de que a persistência também segue RN11/16.
// Se o MySQL não estiver disponível (nenhum servidor configurado), cai de
// volta para SQLite em vez de derrubar a aplicação.
static IClienteRepository EscolherRepositorio()
{
    Console.WriteLine("Qual banco de dados deseja usar?");
    Console.WriteLine("1) SQLite (arquivo local, não exige instalação)");
    Console.WriteLine("2) MySQL (requer servidor MySQL configurado)");
    Console.Write("Escolha uma opção [1]: ");
    var opcao = Console.ReadLine()?.Trim();

    if (opcao == "2")
    {
        Console.Write("Connection string do MySQL (ex.: Server=localhost;Port=3306;Database=cadastro_clientes;User ID=root;Password=SUASENHA;): ");
        var mysqlConnectionString = Console.ReadLine() ?? string.Empty;

        try
        {
            MySqlDatabaseInitializer.Inicializar(mysqlConnectionString);
            Console.WriteLine("Conectado ao MySQL com sucesso.");
            return new MySqlClienteRepository(mysqlConnectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Não foi possível conectar ao MySQL ({ex.Message}).");
            Console.WriteLine("Usando SQLite como alternativa.");
        }
    }

    var caminhoBanco = Path.Combine(AppContext.BaseDirectory, "clientes.db");
    var sqliteConnectionString = $"Data Source={caminhoBanco}";
    DatabaseInitializer.Inicializar(sqliteConnectionString);
    return new SqliteClienteRepository(sqliteConnectionString);
}
