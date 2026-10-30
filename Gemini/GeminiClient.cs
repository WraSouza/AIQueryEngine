using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ServerMCP.Gemini
{
    public class GeminiClient(HttpClient httpClient)
    {
       
        public async Task<string> SendMessageAsync(string userMessage)
        {
            
            var payload = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[] { new { text = userMessage } }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            string conteudo = File.ReadAllText(@"S:\TI\Wladimir\AIQueryEngine\gemini-api-key.txt");
           
            var response = await httpClient.PostAsync(
               "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key=" + conteudo,
               content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.Error.WriteLine($"[ERRO] Gemini API retornou {response.StatusCode}: {errorContent}");
                    return "";
                }


            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var reply = doc.RootElement
                           .GetProperty("candidates")[0]
                           .GetProperty("content")
                           .GetProperty("parts")[0]
                           .GetProperty("text")
                           .GetString();

            return reply ?? "";
        }
    }
}
