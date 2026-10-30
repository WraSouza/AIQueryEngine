using System.Text.Json.Serialization;

namespace AIQueryEngine.Model
{
    public class ChatRequest
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }
}