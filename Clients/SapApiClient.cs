using System.Net.Http.Headers;
using System.Text.Json;
using AIQueryEngine.Helpers;

namespace AIQueryEngine.Clients
{
    public class SapApiClient
    {
        private readonly HttpClient _httpClient;

        public SapApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<string> GetAsync(string query)
        {
            var url =
                $"{SapApiEndpoint.SapApiBaseUrl}?sql={Uri.EscapeDataString(query)}";

            using var request =
                new HttpRequestMessage(HttpMethod.Get, url);

            string conteudo = File.ReadAllText(@"S:\TI\Wladimir\AIQueryEngine\SAPToken.txt");

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    conteudo
                );

            using var response = await _httpClient.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            // Conta os registros quando a resposta é um array JSON.
            try
            {
                using var documento = JsonDocument.Parse(content);
                var raiz = documento.RootElement;

                if (raiz.ValueKind == JsonValueKind.Array)
                {
                    Console.Error.WriteLine(
                        $"[DEBUG] Quantidade de registros recebidos: {raiz.GetArrayLength()}");
                }
                else if (raiz.ValueKind == JsonValueKind.Object)
                {
                    Console.Error.WriteLine(
                        "[DEBUG] A resposta JSON é um objeto, não um array.");

                    if (raiz.TryGetProperty("data", out var data) &&
                        data.ValueKind == JsonValueKind.Array)
                    {
                        Console.Error.WriteLine(
                            $"[DEBUG] Quantidade de registros em 'data': {data.GetArrayLength()}");
                    }
                }
                else
                {
                    Console.Error.WriteLine(
                        $"[DEBUG] Tipo de JSON recebido: {raiz.ValueKind}");
                }
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine(
                    $"[DEBUG] Não foi possível interpretar a resposta como JSON: {ex.Message}");
            }

            response.EnsureSuccessStatusCode();

            return content;
        }
    }
}