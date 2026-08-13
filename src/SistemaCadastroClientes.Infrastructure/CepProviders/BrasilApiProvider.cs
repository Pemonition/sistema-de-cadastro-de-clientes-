using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SistemaCadastroClientes.Domain.Exceptions;
using SistemaCadastroClientes.Domain.Interfaces;
using SistemaCadastroClientes.Domain.Models;

namespace SistemaCadastroClientes.Infrastructure.CepProviders;

// ADAPTER: a BrasilAPI usa nomes de campo em inglês e sinaliza "não
// encontrado" com HTTP 404 (diferente do ViaCEP, que responde 200 com
// "erro": true). Cada provedor lida com sua própria excentricidade aqui
// dentro, sem vazar isso para o resto da aplicação.
public class BrasilApiProvider : ICepProvider
{
    private readonly HttpClient _httpClient;

    public BrasilApiProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string Nome => "BrasilAPI";

    public async Task<ResultadoConsultaCep> ConsultarAsync(string cep)
    {
        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());

        try
        {
            var httpResponse = await _httpClient.GetAsync($"https://brasilapi.com.br/api/cep/v1/{cepLimpo}");

            if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                return ResultadoConsultaCep.NaoEncontrado();

            httpResponse.EnsureSuccessStatusCode();

            var resposta = await httpResponse.Content.ReadFromJsonAsync<BrasilApiResponse>();
            if (resposta is null)
                return ResultadoConsultaCep.NaoEncontrado();

            return ResultadoConsultaCep.ComSucesso(new Endereco
            {
                Cep = resposta.Cep ?? cepLimpo,
                Logradouro = resposta.Street ?? string.Empty,
                Bairro = resposta.Neighborhood ?? string.Empty,
                Cidade = resposta.City ?? string.Empty,
                Estado = resposta.State ?? string.Empty,
            });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            throw new ServicoIndisponivelException(Nome, ex);
        }
    }

    private class BrasilApiResponse
    {
        [JsonPropertyName("cep")] public string? Cep { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }
        [JsonPropertyName("city")] public string? City { get; set; }
        [JsonPropertyName("neighborhood")] public string? Neighborhood { get; set; }
        [JsonPropertyName("street")] public string? Street { get; set; }
    }
}
