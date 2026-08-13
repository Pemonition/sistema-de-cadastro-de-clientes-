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

var caminhoBanco = Path.Combine(AppContext.BaseDirectory, "clientes.db");
var connectionString = $"Data Source={caminhoBanco}";
DatabaseInitializer.Inicializar(connectionString);

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

IClienteRepository clienteRepository = new SqliteClienteRepository(connectionString);

var cepService = new CepConsultaService(providerFactory.ObterPorIndice(0));
var cadastroService = new ClienteCadastroService(clienteRepository);
var consultaService = new ClienteConsultaService(clienteRepository);

var menu = new MenuPrincipal(cadastroService, consultaService, cepService, providerFactory);
await menu.ExecutarAsync();
