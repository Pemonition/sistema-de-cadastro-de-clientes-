namespace SistemaCadastroClientes.Domain.Exceptions;

public class DadosObrigatoriosAusentesException : Exception
{
    public DadosObrigatoriosAusentesException(string campo) : base($"O campo obrigatório '{campo}' não foi informado.")
    {
    }
}
