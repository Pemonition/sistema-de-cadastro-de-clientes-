namespace SistemaCadastroClientes.Domain.Models;

// Resultado de uma consulta de CEP, independente do provedor usado.
// Existe para que "CEP não encontrado" seja um resultado normal do domínio
// (RN06), não uma exceção — reservamos exceções para falhas de comunicação
// (RN07), que são situações realmente excepcionais.
public class ResultadoConsultaCep
{
    public bool Encontrado { get; }
    public Endereco? Endereco { get; }

    private ResultadoConsultaCep(bool encontrado, Endereco? endereco)
    {
        Encontrado = encontrado;
        Endereco = endereco;
    }

    public static ResultadoConsultaCep ComSucesso(Endereco endereco) => new(true, endereco);

    public static ResultadoConsultaCep NaoEncontrado() => new(false, null);
}
