using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SistemaCadastroClientes.Domain.Exceptions;
using SistemaCadastroClientes.Domain.Interfaces;
using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Infrastructure.CepProviders;

// ADAPTER: traduz o formato específico do ViaCEP (campos em português,
// "erro": true quando o CEP não existe) para o Endereco/ResultadoConsultaCep
// comuns do domínio. Esse conhecimento fica preso a esta classe — é por
// isso que RN11 ("regras de cadastro não dependem de uma API específica")
// é satisfeita.
public class ViaCepProvider : ICepProvider
{
    private readonly HttpClient _httpClient;

    public ViaCepProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Nome => "ViaCEP";

    public async Task<ResultadoConsultaCep> ConsultarAsync(string cep)
    {
        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());

        try
        {
            var resposta = await _httpClient.GetFromJsonAsync<ViaCepResponse>($"https://viacep.com.br/ws/{cepLimpo}/json/");

            // O ViaCEP não usa HTTP 404: ele responde 200 com {"erro": true}.
            if (resposta is null || resposta.Erro == true)
                return ResultadoConsultaCep.NaoEncontrado();

            return ResultadoConsultaCep.ComSucesso(new Endereco
            {
                Cep = resposta.Cep ?? cepLimpo,
                Logradouro = resposta.Logradouro ?? string.Empty,
                Bairro = resposta.Bairro ?? string.Empty,
                Cidade = resposta.Localidade ?? string.Empty,
                Estado = resposta.Uf ?? string.Empty,
            });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new ServicoIndisponivelException(Nome, ex);
        }
    }

    // DTO privado: só esta classe conhece o formato de resposta do ViaCEP.
    private class ViaCepResponse
    {
        [JsonPropertyName("cep")] public string? Cep { get; set; }
        [JsonPropertyName("logradouro")] public string? Logradouro { get; set; }
        [JsonPropertyName("bairro")] public string? Bairro { get; set; }
        [JsonPropertyName("localidade")] public string? Localidade { get; set; }
        [JsonPropertyName("uf")] public string? Uf { get; set; }
        [JsonPropertyName("erro")] public bool? Erro { get; set; }
    }
}
