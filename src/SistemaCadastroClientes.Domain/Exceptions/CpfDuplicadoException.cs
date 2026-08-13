namespace SistemaCadastroClientes.Domain.Exceptions;

public class CpfDuplicadoException : Exception
{
    public CpfDuplicadoException(string cpf) : base($"Já existe um cliente cadastrado com o CPF '{cpf}'.")
    {
    }
}
