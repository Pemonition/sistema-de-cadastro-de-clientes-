namespace SistemaCadastroClientes.Domain.Models;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public Endereco Endereco { get; set; } = new();
}
