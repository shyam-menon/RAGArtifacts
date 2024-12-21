using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Core.Models.Chat;

#pragma warning disable SKEXP0001 // Disable warning about experimental features
#pragma warning disable SKEXP0010 // Disable warning about experimental features

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public class RagChatService : IChatService
    {
        private readonly Kernel _kernel;
        private readonly IAssetService _assetService;
        private readonly OpenAITextEmbeddingGenerationService _embeddingService;
        private readonly IMemoryStore _memoryStore;
        private const string MemoryCollectionName = "assets";

        public RagChatService(
            Kernel kernel,
            IAssetService assetService,
            OpenAITextEmbeddingGenerationService embeddingService,
            IMemoryStore memoryStore)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _memoryStore = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
        }

        public async Task InitializeVectorStoreAsync()
        {
            // Get all assets
            var assets = await _assetService.GetAllAssetsAsync();
            if (assets == null || !assets.Any())
            {
                throw new InvalidOperationException("No assets found to initialize vector store");
            }

            // Create or clear the collection
            try
            {
                await _memoryStore.DeleteCollectionAsync(MemoryCollectionName);
            }
            catch
            {
                // Collection might not exist, that's okay
            }
            await _memoryStore.CreateCollectionAsync(MemoryCollectionName);

            // Add each asset to the vector store
            foreach (var asset in assets.Where(a => !string.IsNullOrEmpty(a.MarkdownContent)))
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(asset.MarkdownContent);
                
                var record = new MemoryRecord(
                    key: asset.Id,
                    metadata: new MemoryRecordMetadata(
                        isReference: true,
                        id: asset.Id,
                        text: asset.MarkdownContent,
                        description: asset.Title ?? "Untitled",
                        additionalMetadata: string.Empty,
                        externalSourceName: "Technical docs"
                    ),
                    embedding: embedding.ToArray(),
                    timestamp: DateTimeOffset.UtcNow
                );

                await _memoryStore.UpsertAsync(MemoryCollectionName, record);
            }
        }

        public async Task<ChatResponse> QueryAsync(ChatRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Query))
            {
                throw new ArgumentException("Query cannot be empty", nameof(request));
            }

            // 1. Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Query);

            // 2. Search for relevant content
            var searchResults = new List<(MemoryRecord Record, double Relevance)>();
            await foreach (var result in _memoryStore.GetNearestMatchesAsync(
                MemoryCollectionName,
                queryEmbedding.ToArray(),
                limit: 3,
                minRelevanceScore: 0.7))
            {
                searchResults.Add(result);
            }

            if (!searchResults.Any())
            {
                return new ChatResponse
                {
                    Response = "I couldn't find any relevant information to answer your question.",
                    Sources = new List<AssetReference>()
                };
            }

            // 3. Format context from search results
            var context = string.Join("\n\n", searchResults.Select(r => 
                $"Document: {r.Record.Metadata.Description}\n{r.Record.Metadata.Text}"));

            // 4. Create chat history
            var chatHistory = new ChatHistory();
            
            foreach (var msg in request.History ?? Enumerable.Empty<ChatMessage>())
            {
                chatHistory.AddMessage(
                    msg.Role.ToLower() == "user" ? AuthorRole.User : AuthorRole.Assistant, 
                    msg.Content);
            }

            // 5. Add RAG prompt
            var prompt = $"""
                Use the following information to answer the question.
                If you cannot answer based on the provided information, say "I don't have enough information to answer that question."

                Context:
                {context}

                Question: {request.Query}

                Answer in a clear and concise way, citing specific information from the provided context when relevant.
                """;
            chatHistory.AddUserMessage(prompt);

            // 6. Get AI response
            var response = await _kernel.GetRequiredService<IChatCompletionService>()
                .GetChatMessageContentAsync(chatHistory);

            // 7. Return response with sources
            return new ChatResponse
            {
                Response = response.Content,
                Sources = searchResults.Select(r => new AssetReference
                {
                    Id = r.Record.Metadata.Id,
                    Title = r.Record.Metadata.Description ?? "Untitled",
                    Snippet = GetRelevantSnippet(r.Record.Metadata.Text ?? string.Empty, request.Query),
                    Relevance = (float)r.Relevance
                }).ToList()
            };
        }

        private string GetRelevantSnippet(string text, string query, int snippetLength = 200)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            // Simple snippet extraction - could be improved with more sophisticated relevance detection
            var words = text.Split(' ');
            if (words.Length <= snippetLength)
                return text;

            // For now, just return the first snippetLength words
            return string.Join(" ", words.Take(snippetLength)) + "...";
        }
    }
}
