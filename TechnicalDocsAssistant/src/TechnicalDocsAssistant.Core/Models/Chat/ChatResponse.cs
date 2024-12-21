using System.Collections.Generic;

namespace TechnicalDocsAssistant.Core.Models.Chat
{
    public class ChatResponse
    {
        public string Response { get; set; }
        public List<AssetReference> Sources { get; set; } = new List<AssetReference>();
    }

    public class AssetReference
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
        public float Relevance { get; set; }
    }
}
