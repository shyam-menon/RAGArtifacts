using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TechnicalDocsAssistant.Core.Models.Chat
{
    public class ChatResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; }
        [JsonPropertyName("sources")]
        public List<AssetReference> Sources { get; set; } = new List<AssetReference>();
    }

    public class AssetReference
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("snippet")]
        public string Snippet { get; set; }
        [JsonPropertyName("relevance")]
        public float Relevance { get; set; }
    }
}
