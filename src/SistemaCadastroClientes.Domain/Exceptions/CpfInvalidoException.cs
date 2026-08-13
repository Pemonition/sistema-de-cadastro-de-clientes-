namespace SistemaCadastroClientes.Domain.Exceptions;

public class CpfInvalidoException : Exception
{
    public CpfInvalidoException(string cpf) : base($"O CPF '{cpf}' informado não é válido.")
    {
    }
}
