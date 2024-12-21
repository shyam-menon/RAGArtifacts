using System.Threading.Tasks;
using TechnicalDocsAssistant.Core.Models.Chat;

namespace TechnicalDocsAssistant.Core.Interfaces
{
    public interface IChatService
    {
        /// <summary>
        /// Processes a chat query using RAG (Retrieval Augmented Generation)
        /// </summary>
        /// <param name="request">The chat request containing the query and conversation history</param>
        /// <returns>A response containing the AI's answer and relevant source references</returns>
        Task<ChatResponse> QueryAsync(ChatRequest request);

        /// <summary>
        /// Initializes the vector store with embeddings for all assets
        /// </summary>
        Task InitializeVectorStoreAsync();
    }
}
