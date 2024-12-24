#pragma warning disable SKEXP0010 // Disable warning about experimental features
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0020 // API is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0050 // API is for evaluation purposes only and is subject to change or removal in future updates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Core.Models.Chat;
using Microsoft.Extensions.DependencyInjection;

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public class SimpleChatService : IChatService
    {
        private readonly Kernel _kernel;
        private readonly IAssetService _assetService;
        private readonly OpenAITextEmbeddingGenerationService _embeddingService;
        private readonly Dictionary<string, (float[] Embedding, string Content)> _vectorStore;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly UserStoryAgent _userStoryAgent;

        public SimpleChatService(
            Kernel kernel,
            IAssetService assetService,
            OpenAITextEmbeddingGenerationService embeddingService)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
            _embeddingService = embeddingService ?? throw new ArgumentNullException(nameof(embeddingService));
            _vectorStore = new Dictionary<string, (float[] Embedding, string Content)>();
            _chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            _userStoryAgent = new UserStoryAgent(_chatCompletionService, assetService);
        }

        public async Task InitializeVectorStoreAsync()
        {
            Console.WriteLine("Initializing vector store");
            var assets = await _assetService.GetAllAssetsAsync();
            var assetsList = assets.ToList();
            Console.WriteLine($"Found {assetsList.Count} assets");

            foreach (var asset in assetsList)
            {
                Console.WriteLine($"Asset: {asset.Title}");
                if (asset.ContentVector != null)
                {
                    Console.WriteLine($"Asset has embedding with dimension: {asset.ContentVector.Length}");
                    Console.WriteLine($"First few values: {string.Join(", ", asset.ContentVector.Take(5))}");
                }
                else
                {
                    Console.WriteLine("Asset has no embedding");
                }
            }
        }

        public async Task<ChatResponse> QueryAsync(ChatRequest request)
        {
            // Get the embedding generation service
            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

            // Generate embeddings for the query
            Console.WriteLine($"Generating embeddings for query: {request.Query}");
            var queryEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(request.Query);
            var queryVector = queryEmbedding.ToArray();
            Console.WriteLine($"Generated query embedding with dimension: {queryVector.Length}");
            Console.WriteLine($"First few values: {string.Join(", ", queryVector.Take(5))}");

            // Check if this is a user story request
            if (InputAnalyzer.IsUserStoryRequest(request.Query))
            {
                var userStoryJson = await _userStoryAgent.GenerateUserStory(request.Query, queryVector);
                var formattedUserStory = $@"<div class=""user-story-response"">
{userStoryJson}
</div>";
                return new ChatResponse { Response = formattedUserStory };
            }

            // Get similar documents
            Console.WriteLine("Getting similar documents");
            var similarityThreshold = 0.2f;  // Lower threshold for semantic similarity
            var limit = 10;  // Get more candidates initially
            var similarDocs = await _assetService.GetSimilarAssetsAsync(queryVector, limit, similarityThreshold);
            
            // Post-process the results
            var relevantDocs = similarDocs
                .Where(doc => !string.IsNullOrEmpty(doc.MarkdownContent))
                .OrderByDescending(doc => doc.Similarity)
                .Take(5)
                .ToList();

            Console.WriteLine($"Found {relevantDocs.Count} relevant documents");
            foreach (var doc in relevantDocs)
            {
                Console.WriteLine($"Document: {doc.Title}");
                Console.WriteLine($"Content: {doc.MarkdownContent}");
                if (doc.ContentVector != null)
                {
                    Console.WriteLine($"Document has embedding with dimension: {doc.ContentVector.Length}");
                    Console.WriteLine($"First few values: {string.Join(", ", doc.ContentVector.Take(5))}");
                }
                else
                {
                    Console.WriteLine("Document has no embedding");
                }
            }

            if (!relevantDocs.Any())
            {
                return new ChatResponse
                {
                    Response = "I couldn't find any relevant information to answer your question. Please try rephrasing your query or ask something else.",
                    Sources = new List<AssetReference>()
                };
            }

            // Construct the system message with context
            var context = string.Join("\n\n", relevantDocs.Select(d => d.MarkdownContent));
            var systemMessage = $@"You are a helpful assistant that answers questions based on the provided context. 
Follow these guidelines when responding:
1. Format your responses in plain text, avoiding any markdown or special formatting
2. Use simple bullet points (- ) or numbers (1. ) for lists
3. Keep technical terms in their plain form without any special formatting
4. Break down complex information into clear sections using line breaks
5. If the context doesn't contain relevant information to answer the question, say so
6. Be concise but thorough
7. Maintain a professional tone
8. Use indentation with spaces for nested lists or hierarchical information

Here is the context:

{context}";

            // Create chat completion options
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            // Create the chat history
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(systemMessage);

            // Add previous messages
            if (request.History != null)
            {
                foreach (var message in request.History)
                {
                    if (message.Role == "user")
                    {
                        chatHistory.AddUserMessage(message.Content);
                    }
                    else if (message.Role == "assistant")
                    {
                        chatHistory.AddAssistantMessage(message.Content);
                    }
                }
            }

            // Add the current query
            chatHistory.AddUserMessage(request.Query);

            // Get the response
            var chatResult = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
            var response = chatResult.Content;

            // Convert assets to asset references with better snippets
            var assetReferences = relevantDocs.Select(doc => new AssetReference
            {
                Id = doc.Id,
                Title = doc.Title,
                Snippet = GetRelevantSnippet(doc.MarkdownContent, request.Query, 300),
                Relevance = doc.Similarity ?? 0.0f  // Use actual similarity score, default to 0 if null
            })
            .Where(r => r.Relevance > 0.0f)  // Only include sources with positive relevance
            .OrderByDescending(r => r.Relevance)  // Order by actual relevance score
            .ToList();

            return new ChatResponse
            {
                Response = response,
                Sources = assetReferences
            };
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Vectors must have the same dimension");

            float dotProduct = 0;
            float normA = 0;
            float normB = 0;

            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }

            if (normA == 0 || normB == 0)
                return 0;

            return dotProduct / (float)(Math.Sqrt(normA) * Math.Sqrt(normB));
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
