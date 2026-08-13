namespace SistemaCadastroClientes.Domain.Exceptions;

// Lançada por um ICepProvider quando a comunicação com o serviço externo
// falha (timeout, DNS, HTTP 5xx, JSON corrompido, etc.) — RN07.
// É uma exceção de domínio: a camada de aplicação a captura e traduz em
// mensagem amigável, sem vazar detalhes técnicos ao usuário.
public class ServicoIndisponivelException : Exception
{
    public ServicoIndisponivelException(string provedor, Exception inner)
        : base($"O provedor de CEP '{provedor}' está indisponível no momento.", inner)
    {
    }
}
