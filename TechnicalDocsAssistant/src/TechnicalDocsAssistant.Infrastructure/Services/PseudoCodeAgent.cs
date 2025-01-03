using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Core.Interfaces;

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public class PseudoCodeAgent
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IAssetService _assetService;

        public PseudoCodeAgent(IChatCompletionService chatCompletionService, IAssetService assetService)
        {
            _chatCompletionService = chatCompletionService;
            _assetService = assetService;
        }

        public async Task<string> GeneratePseudoCode(string input, float[] queryVector)
        {
            try
            {
                // 1. Extract user story from input
                var userStory = input.Replace("Code:", "", StringComparison.OrdinalIgnoreCase).Trim();
                Console.WriteLine($"Processing user story: {userStory}");

                // 2. Find relevant assets using embeddings
                var similarityThreshold = 0.3f;
                var limit = 3;
                var relevantAssets = await _assetService.GetSimilarAssetsAsync(queryVector, limit, similarityThreshold);
                
                if (!relevantAssets.Any())
                {
                    Console.WriteLine("No relevant assets found");
                    return GenerateGenericImplementation(userStory);
                }

                // 3. Extract technology stack from the most relevant asset
                var primaryAsset = relevantAssets.First();
                var techStack = ExtractTechnologyStack(primaryAsset);

                Console.WriteLine($"Found asset: {primaryAsset.Title}");
                Console.WriteLine($"Technology stack - Frontend: {techStack.Frontend}, Backend: {techStack.Backend}, Database: {techStack.Database}");

                // 4. Generate implementation based on available information
                if (HasValidTechnologyStack(techStack))
                {
                    return await GenerateImplementationWithStack(userStory, techStack, primaryAsset.MarkdownContent);
                }

                return GenerateGenericImplementation(userStory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating pseudocode: {ex.Message}");
                return JsonSerializer.Serialize(new
                {
                    title = "Error Generating Implementation",
                    error = ex.Message,
                    description = "An error occurred while generating the implementation. Please try again."
                });
            }
        }

        private TechnologyStack ExtractTechnologyStack(Asset asset)
        {
            var stack = new TechnologyStack();

            // For MPC, use predefined stack
            if (IsMpcAsset(asset))
            {
                stack.Frontend = "Angular";
                stack.Backend = ".NET Core Web API";
                stack.Database = "PostgreSQL";
                return stack;
            }

            // Try to extract from content
            var content = asset.MarkdownContent ?? string.Empty;

            if (content.Contains("Angular", StringComparison.OrdinalIgnoreCase))
                stack.Frontend = "Angular";
            else if (content.Contains("React", StringComparison.OrdinalIgnoreCase))
                stack.Frontend = "React";

            if (content.Contains(".NET Core", StringComparison.OrdinalIgnoreCase))
                stack.Backend = ".NET Core Web API";
            else if (content.Contains("Node.js", StringComparison.OrdinalIgnoreCase))
                stack.Backend = "Node.js";

            if (content.Contains("PostgreSQL", StringComparison.OrdinalIgnoreCase))
                stack.Database = "PostgreSQL";
            else if (content.Contains("SQL Server", StringComparison.OrdinalIgnoreCase))
                stack.Database = "SQL Server";

            return stack;
        }

        private bool IsMpcAsset(Asset asset)
        {
            return asset.Title?.Contains("MPC", StringComparison.OrdinalIgnoreCase) == true ||
                   asset.MarkdownContent?.Contains("Managed Print Central", StringComparison.OrdinalIgnoreCase) == true ||
                   asset.MarkdownContent?.Contains("MPC", StringComparison.OrdinalIgnoreCase) == true;
        }

        private bool HasValidTechnologyStack(TechnologyStack stack)
        {
            return !string.IsNullOrEmpty(stack.Frontend) ||
                   !string.IsNullOrEmpty(stack.Backend) ||
                   !string.IsNullOrEmpty(stack.Database);
        }

        private async Task<string> GenerateImplementationWithStack(string userStory, TechnologyStack stack, string context)
        {
            var prompt = $@"Generate a technical implementation for this user story using the specified technology stack.

User Story: {userStory}

Technology Stack:
- Frontend: {stack.Frontend ?? "Not specified"}
- Backend: {stack.Backend ?? "Not specified"}
- Database: {stack.Database ?? "Not specified"}

Available Context:
{context}

Return a JSON response with this structure:
{{
    ""title"": ""Implementation Title"",
    ""technologyStack"": {{
        ""frontend"": ""{stack.Frontend}"",
        ""backend"": ""{stack.Backend}"",
        ""database"": ""{stack.Database}""
    }},
    ""components"": [
        {{
            ""name"": ""Component Name"",
            ""type"": ""Frontend|Backend|Database"",
            ""language"": ""typescript|csharp|sql"",
            ""fileName"": ""filename.ext"",
            ""code"": [""Actual implementation code with imports""],
            ""description"": ""Component description""
        }}
    ],
    ""dataModels"": [
        {{
            ""name"": ""Model Name"",
            ""language"": ""typescript|csharp"",
            ""code"": ""Complete model definition"",
            ""properties"": [
                {{
                    ""name"": ""propertyName"",
                    ""type"": ""propertyType"",
                    ""description"": ""Property description""
                }}
            ]
        }}
    ]
}}";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(prompt);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);
            return response.Content;
        }

        private string GenerateGenericImplementation(string userStory)
        {
            return JsonSerializer.Serialize(new
            {
                title = "Generic Implementation Guide",
                description = "Technology stack could not be determined. Here's a generic implementation guide.",
                steps = new[]
                {
                    new { step = 1, title = "Frontend Implementation", description = "Create a user interface that:" },
                    new { step = 2, title = "Backend API", description = "Implement REST endpoints that:" },
                    new { step = 3, title = "Data Storage", description = "Design a database schema that:" },
                    new { step = 4, title = "Integration", description = "Connect the components:" }
                },
                note = "This is a generic implementation guide. Once the technology stack is identified, a more specific implementation can be generated."
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public class TechnologyStack
    {
        public string Frontend { get; set; }
        public string Backend { get; set; }
        public string Database { get; set; }
    }
}
