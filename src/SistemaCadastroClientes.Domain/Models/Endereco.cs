namespace SistemaCadastroClientes.Domain.Models;

// Representação ÚNICA e interna de um endereço.
// Cada provedor de CEP (ViaCEP, BrasilAPI, ...) devolve um JSON com formato
// próprio; é responsabilidade de cada provedor (na camada Infrastructure)
// converter sua resposta para este tipo. O resto do sistema nunca vê o
// formato original de nenhuma API — só enxerga um Endereco.
public class Endereco
{
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
