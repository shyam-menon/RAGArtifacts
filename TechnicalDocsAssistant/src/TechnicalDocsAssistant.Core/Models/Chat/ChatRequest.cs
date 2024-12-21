using System.Collections.Generic;

namespace TechnicalDocsAssistant.Core.Models.Chat
{
    public class ChatRequest
    {
        public string Query { get; set; }
        public List<ChatMessage> History { get; set; } = new List<ChatMessage>();
    }
}
