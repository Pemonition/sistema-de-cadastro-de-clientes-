namespace SistemaCadastroClientes.Application.Validation;

// Implementa o algoritmo oficial de validação de CPF (módulo 11).
// Fica isolado em uma classe estática pequena e sem dependências: é fácil
// de testar (entrada -> saída, sem I/O) e fácil de reaproveitar.
public static class CpfValidator
{
    public static bool EhValido(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;

        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11) return false;

        // Sequências como "11111111111" passam no cálculo do dígito
        // verificador, mas não são CPFs válidos na prática.
        if (digitos.Distinct().Count() == 1) return false;

        var numeros = digitos.Select(c => c - '0').ToArray();

        int primeiroDigito = CalcularDigitoVerificador(numeros, 9);
        if (primeiroDigito != numeros[9]) return false;

        int segundoDigito = CalcularDigitoVerificador(numeros, 10);
        if (segundoDigito != numeros[10]) return false;

        return true;
    }

    private static int CalcularDigitoVerificador(int[] numeros, int quantidadeDigitos)
    {
        int peso = quantidadeDigitos + 1;
        int soma = 0;
        for (int i = 0; i < quantidadeDigitos; i++)
        {
            soma += numeros[i] * peso;
            peso--;
        }

        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    // Remove máscara (pontos e traço), útil antes de persistir/comparar.
    public static string Normalizar(string cpf) => new(cpf.Where(char.IsDigit).ToArray());
}
