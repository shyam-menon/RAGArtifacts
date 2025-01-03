#pragma warning disable SKEXP0010 // Disable warning about experimental features
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0020 // API is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0050 // API is for evaluation purposes only and is subject to change or removal in future updates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private readonly PseudoCodeAgent _pseudoCodeAgent;

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
            _pseudoCodeAgent = new PseudoCodeAgent(_chatCompletionService, assetService);
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

            // Check if this is a pseudocode request
            if (InputAnalyzer.IsPseudoCodeRequest(request.Query))
            {
                Console.WriteLine("Handling pseudocode request");
                try
                {
                    var pseudoCodeJson = await _pseudoCodeAgent.GeneratePseudoCode(request.Query, queryVector);
                    if (string.IsNullOrEmpty(pseudoCodeJson))
                    {
                        Console.WriteLine("No pseudocode was generated");
                        return new ChatResponse { Response = "Failed to generate pseudocode. Please try again." };
                    }
                    var formattedPseudoCode = FormatPseudoCodeResponse(pseudoCodeJson);
                    return new ChatResponse { 
                        Response = $"Here's the implementation:\n\n{formattedPseudoCode}",
                        Sources = new List<AssetReference>()
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating pseudocode: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    return new ChatResponse { Response = "An error occurred while generating pseudocode. Please try again." };
                }
            }

            // Get similar documents
            Console.WriteLine("Getting similar documents");
            var similarityThreshold = 0.1f;  // Lower threshold to see what's coming back
            var limit = 10;  // Get more candidates initially
            var similarDocs = await _assetService.GetSimilarAssetsAsync(queryVector, limit, similarityThreshold);
            
            Console.WriteLine($"Retrieved {similarDocs.Count()} documents from vector search");
            foreach (var doc in similarDocs)
            {
                Console.WriteLine($"Document: {doc.Title}, Similarity: {doc.Similarity?.ToString("F4") ?? "null"}, Has Content: {!string.IsNullOrEmpty(doc.MarkdownContent)}");
            }
            
            // Post-process the results
            var relevantDocs = similarDocs
                .Where(doc => !string.IsNullOrEmpty(doc.MarkdownContent))
                .OrderByDescending(doc => doc.Similarity ?? 0.0f)  // Handle null similarity
                .Take(5)
                .ToList();

            Console.WriteLine($"After filtering, found {relevantDocs.Count} relevant documents");
            foreach (var doc in relevantDocs)
            {
                Console.WriteLine($"Relevant doc: {doc.Title}, Similarity: {doc.Similarity?.ToString("F4") ?? "null"}");
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
            var assetReferences = relevantDocs
                .OrderByDescending(doc => doc.Similarity ?? 0.0f)
                .Take(1)  // Take only the most relevant document
                .Select(doc => new AssetReference
                {
                    Id = doc.Id,
                    Title = doc.Title,
                    Snippet = GetRelevantSnippet(doc.MarkdownContent, request.Query, 300),
                    Relevance = doc.Similarity ?? 0.01f
                })
                .ToList();

            Console.WriteLine($"Final asset reference: {(assetReferences.FirstOrDefault()?.Title ?? "none")}, " +
                            $"Relevance: {assetReferences.FirstOrDefault()?.Relevance.ToString("F4") ?? "n/a"}");

            return new ChatResponse
            {
                Response = response,
                Sources = assetReferences
            };
        }

        private string FormatPseudoCodeResponse(string pseudoCodeJson)
        {
            try
            {
                if (pseudoCodeJson.StartsWith("```json"))
                {
                    pseudoCodeJson = pseudoCodeJson.Substring(7);
                }
                if (pseudoCodeJson.EndsWith("```"))
                {
                    pseudoCodeJson = pseudoCodeJson.Substring(0, pseudoCodeJson.Length - 3);
                }
                pseudoCodeJson = pseudoCodeJson.Trim();
                
                Console.WriteLine($"Formatting pseudocode JSON: {pseudoCodeJson}");
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                };
                
                var implementation = JsonSerializer.Deserialize<ImplementationResponse>(pseudoCodeJson, options);
                
                if (implementation == null)
                {
                    Console.WriteLine("Failed to deserialize implementation response");
                    return "Error: Failed to format pseudocode response.";
                }

                var sb = new StringBuilder();
                
                // Title
                sb.AppendLine($"# {implementation.Title}");
                sb.AppendLine();
                
                // Technology Stack
                sb.AppendLine("## Technology Stack");
                if (implementation.TechnologyStack != null)
                {
                    sb.AppendLine($"- Frontend: {implementation.TechnologyStack.Frontend}");
                    sb.AppendLine($"- Backend: {implementation.TechnologyStack.Backend}");
                    sb.AppendLine($"- Database: {implementation.TechnologyStack.Database}");
                }
                sb.AppendLine();
                
                // Components
                if (implementation.Components?.Length > 0)
                {
                    sb.AppendLine("## Components");
                    foreach (var component in implementation.Components)
                    {
                        if (component == null) continue;
                        
                        sb.AppendLine($"### {component.Name} ({component.Type})");
                        if (!string.IsNullOrEmpty(component.FileName))
                        {
                            sb.AppendLine($"File: `{component.FileName}`");
                            sb.AppendLine();
                        }
                        
                        if (!string.IsNullOrEmpty(component.Description))
                        {
                            sb.AppendLine(component.Description);
                            sb.AppendLine();
                        }
                        
                        if (component.Code?.Length > 0)
                        {
                            sb.AppendLine($"```{component.Language.ToLowerInvariant()}");
                            foreach (var codeLine in component.Code)
                            {
                                sb.AppendLine(codeLine);
                            }
                            sb.AppendLine("```");
                            sb.AppendLine();
                        }
                    }
                }

                // Data Models
                if (implementation.DataModels?.Length > 0)
                {
                    sb.AppendLine("## Data Models");
                    foreach (var model in implementation.DataModels)
                    {
                        if (model == null) continue;
                        
                        sb.AppendLine($"### {model.Name}");
                        sb.AppendLine();
                        
                        if (!string.IsNullOrEmpty(model.Code))
                        {
                            sb.AppendLine($"```{model.Language.ToLowerInvariant()}");
                            sb.AppendLine(model.Code);
                            sb.AppendLine("```");
                            sb.AppendLine();
                        }
                        
                        if (model.Properties?.Length > 0)
                        {
                            sb.AppendLine("Properties:");
                            foreach (var prop in model.Properties)
                            {
                                if (prop == null) continue;
                                sb.AppendLine($"- {prop.Name} ({prop.Type}): {prop.Description}");
                            }
                            sb.AppendLine();
                        }
                    }
                }

                // API Interfaces
                if (implementation.Interfaces?.Length > 0)
                {
                    sb.AppendLine("## API Interfaces");
                    foreach (var iface in implementation.Interfaces)
                    {
                        if (iface == null) continue;
                        
                        sb.AppendLine($"### {iface.Name}");
                        if (!string.IsNullOrEmpty(iface.Method))
                            sb.AppendLine($"- Method: {iface.Method}");
                        if (!string.IsNullOrEmpty(iface.Route))
                            sb.AppendLine($"- Route: `{iface.Route}`");
                        sb.AppendLine();
                        
                        if (!string.IsNullOrEmpty(iface.Code))
                        {
                            sb.AppendLine("```");
                            sb.AppendLine(iface.Code);
                            sb.AppendLine("```");
                            sb.AppendLine();
                        }
                        
                        if (!string.IsNullOrEmpty(iface.Description))
                        {
                            sb.AppendLine(iface.Description);
                            sb.AppendLine();
                        }
                    }
                }

                var result = sb.ToString();
                Console.WriteLine("Formatted output:");
                Console.WriteLine(result);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error formatting pseudocode: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"Original JSON: {pseudoCodeJson}");
                return $"Error formatting pseudocode response: {ex.Message}";
            }
        }

        private class ArrayConverter<T> : JsonConverter<T[]>
        {
            public override T[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType == JsonTokenType.Null)
                    return Array.Empty<T>();

                var list = JsonSerializer.Deserialize<List<T>>(ref reader, options);
                return list?.ToArray() ?? Array.Empty<T>();
            }

            public override void Write(Utf8JsonWriter writer, T[] value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, options);
            }
        }

        private class ImplementationResponse
        {
            [JsonPropertyName("title")]
            public string Title { get; set; } = string.Empty;
            
            [JsonPropertyName("technologyStack")]
            public TechnologyStack TechnologyStack { get; set; } = new TechnologyStack();
            
            [JsonPropertyName("components")]
            [JsonConverter(typeof(ArrayConverter<Component>))]
            public Component[] Components { get; set; } = Array.Empty<Component>();
            
            [JsonPropertyName("dataModels")]
            [JsonConverter(typeof(ArrayConverter<DataModel>))]
            public DataModel[] DataModels { get; set; } = Array.Empty<DataModel>();
            
            [JsonPropertyName("interfaces")]
            [JsonConverter(typeof(ArrayConverter<Interface>))]
            public Interface[] Interfaces { get; set; } = Array.Empty<Interface>();
        }

        private class TechnologyStack
        {
            [JsonPropertyName("frontend")]
            public string Frontend { get; set; } = string.Empty;
            
            [JsonPropertyName("backend")]
            public string Backend { get; set; } = string.Empty;
            
            [JsonPropertyName("database")]
            public string Database { get; set; } = string.Empty;
        }

        private class Component
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
            
            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;
            
            [JsonPropertyName("language")]
            public string Language { get; set; } = string.Empty;
            
            [JsonPropertyName("fileName")]
            public string FileName { get; set; } = string.Empty;
            
            [JsonPropertyName("code")]
            [JsonConverter(typeof(ArrayConverter<string>))]
            public string[] Code { get; set; } = Array.Empty<string>();
            
            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
        }

        private class DataModel
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
            
            [JsonPropertyName("language")]
            public string Language { get; set; } = string.Empty;
            
            [JsonPropertyName("code")]
            public string Code { get; set; } = string.Empty;
            
            [JsonPropertyName("properties")]
            [JsonConverter(typeof(ArrayConverter<Property>))]
            public Property[] Properties { get; set; } = Array.Empty<Property>();
        }

        private class Property
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
            
            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;
            
            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
        }

        private class Interface
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
            
            [JsonPropertyName("method")]
            public string Method { get; set; } = string.Empty;
            
            [JsonPropertyName("route")]
            public string Route { get; set; } = string.Empty;
            
            [JsonPropertyName("code")]
            public string Code { get; set; } = string.Empty;
            
            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
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

        private float CosineSimilarity(float[] a, float[] b)
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
    }
}
