using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SistemaCadastroClientes.Domain.Exceptions;
using SistemaCadastroClientes.Domain.Interfaces;
using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Infrastructure.CepProviders;

// TERCEIRO PROVEDOR — "Desafio Final" da especificação (seção 24).
// A diretoria pediu mais uma fonte de CEP; a resposta arquitetural é esta
// classe inteira. Ela implementa a mesma ICepProvider dos outros dois e é
// registrada com uma linha a mais em Program.cs (o composition root).
// Nada em Domain, Application ou MenuPrincipal precisou mudar — é
// exatamente o que RN11/RN12 exigiam.
public class OpenCepProvider : ICepProvider
{
    private readonly HttpClient _httpClient;

    public OpenCepProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Nome => "OpenCEP";

    public async Task<ResultadoConsultaCep> ConsultarAsync(string cep)
    {
        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());

        try
        {
            var httpResponse = await _httpClient.GetAsync($"https://opencep.com/v1/{cepLimpo}");

            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                return ResultadoConsultaCep.NaoEncontrado();

            httpResponse.EnsureSuccessStatusCode();

            var resposta = await httpResponse.Content.ReadFromJsonAsync<OpenCepResponse>();
            if (resposta is null)
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

    private class OpenCepResponse
    {
        [JsonPropertyName("cep")] public string? Cep { get; set; }
        [JsonPropertyName("logradouro")] public string? Logradouro { get; set; }
        [JsonPropertyName("bairro")] public string? Bairro { get; set; }
        [JsonPropertyName("localidade")] public string? Localidade { get; set; }
        [JsonPropertyName("uf")] public string? Uf { get; set; }
    }
}
