namespace SistemaCadastroClientes.Domain.Exceptions;

// Lançada pelo repositório quando o acesso ao banco de dados falha.
// Assim como ServicoIndisponivelException, existe para que a UI trate uma
// falha técnica com uma mensagem amigável em vez de deixar a exceção crua
// (SqliteException, etc.) vazar para fora da camada de infraestrutura.
public class PersistenciaException : Exception
{
    public PersistenciaException(string mensagem, Exception inner) : base(mensagem, inner)
    {
    }
}
