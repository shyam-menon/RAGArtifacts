using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TechnicalDocsAssistant.Core.Models.Chat
{
    public class ChatRequest
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }
        [JsonPropertyName("history")]
        public List<ChatMessage> History { get; set; } = new List<ChatMessage>();
    }
}
