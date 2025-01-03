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
            // Parse key actions and entities from the user story
            var actions = ExtractActionsFromUserStory(userStory);
            var entities = ExtractEntitiesFromUserStory(userStory);

            return JsonSerializer.Serialize(new
            {
                title = "Implementation Guide Based on User Story",
                userStory = userStory,
                description = "Implementation guide generated from user story analysis.",
                steps = GenerateImplementationSteps(actions, entities),
                dataModel = GenerateDataModel(entities),
                technicalRequirements = new
                {
                    authentication = RequiresAuthentication(userStory),
                    dataStorage = RequiresDataStorage(entities),
                    externalIntegration = RequiresExternalIntegration(userStory)
                }
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        private object[] GenerateImplementationSteps(string[] actions, string[] entities)
        {
            var steps = new List<object>();

            // User Interface Layer
            steps.Add(new
            {
                step = 1,
                title = "User Interface Layer",
                description = $"Implement user interface components to handle: {string.Join(", ", actions)}",
                requirements = new[]
                {
                    "Create responsive dashboard layout",
                    $"Implement views for {string.Join(" and ", entities)}",
                    "Design navigation structure",
                    "Add user feedback mechanisms",
                    "Ensure accessibility compliance",
                    "Implement responsive design for all screen sizes"
                }
            });

            // Application Layer
            steps.Add(new
            {
                step = 2,
                title = "Application Layer",
                description = "Implement business logic and API endpoints:",
                requirements = new[]
                {
                    $"Create service interfaces for {string.Join(", ", entities)}",
                    "Implement authentication and authorization",
                    "Add request validation",
                    "Implement error handling",
                    "Add logging and monitoring",
                    "Implement caching strategy"
                }
            });

            // Data Access Layer
            if (RequiresDataStorage(entities))
            {
                steps.Add(new
                {
                    step = 3,
                    title = "Data Access Layer",
                    description = "Design and implement data storage:",
                    requirements = new[]
                    {
                        $"Define data models for: {string.Join(", ", entities)}",
                        "Implement repository pattern",
                        "Add data validation",
                        "Implement data access security",
                        "Add database indexing strategy",
                        "Implement data caching"
                    }
                });
            }

            // Integration Layer
            steps.Add(new
            {
                step = 4,
                title = "Integration Layer",
                description = "Connect and test all components:",
                requirements = new[]
                {
                    "Implement dependency injection",
                    "Add unit tests for each layer",
                    "Implement integration tests",
                    "Add performance monitoring",
                    "Implement health checks",
                    "Set up continuous integration"
                }
            });

            return steps.ToArray();
        }

        private object GenerateDataModel(string[] entities)
        {
            return entities.Select(entity => new
            {
                name = char.ToUpper(entity[0]) + entity.Substring(1),
                properties = GeneratePropertiesForEntity(entity)
            }).ToArray();
        }

        private object[] GeneratePropertiesForEntity(string entity)
        {
            var commonProperties = new[]
            {
                new { name = "Id", type = "string", description = "Unique identifier" },
                new { name = "Name", type = "string", description = $"Name of the {entity}" },
                new { name = "CreatedAt", type = "datetime", description = "Creation timestamp" },
                new { name = "UpdatedAt", type = "datetime", description = "Last update timestamp" }
            };

            return commonProperties;
        }

        private string DetermineHttpMethod(string action)
        {
            return action.ToLower() switch
            {
                var a when a.Contains("create") || a.Contains("add") => "POST",
                var a when a.Contains("update") || a.Contains("edit") => "PUT",
                var a when a.Contains("delete") || a.Contains("remove") => "DELETE",
                var a when a.Contains("view") || a.Contains("list") || a.Contains("search") => "GET",
                _ => "GET"
            };
        }

        private bool RequiresAuthentication(string userStory)
        {
            var authTerms = new[] { "login", "auth", "secure", "user", "account", "permission" };
            return authTerms.Any(term => userStory.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private bool RequiresDataStorage(string[] entities)
        {
            return entities.Length > 0;
        }

        private bool RequiresExternalIntegration(string userStory)
        {
            var integrationTerms = new[] { "integrate", "connect", "api", "external", "service", "third-party" };
            return integrationTerms.Any(term => userStory.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private string[] ExtractActionsFromUserStory(string userStory)
        {
            // Common action verbs in user stories
            var actionVerbs = new[] { "view", "create", "update", "delete", "manage", "access", "see", "edit", "remove", "add", "list", "search", "filter" };
            return actionVerbs.Where(verb => userStory.Contains(verb, StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        private string[] ExtractEntitiesFromUserStory(string userStory)
        {
            // Extract nouns that could be entities (simplified approach)
            var words = userStory.Split(' ');
            var commonEntities = new[] { "user", "account", "data", "information", "profile", "customer", "partner", "fleet" };
            return commonEntities.Where(entity => userStory.Contains(entity, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
    }

    public class TechnologyStack
    {
        public string Frontend { get; set; }
        public string Backend { get; set; }
        public string Database { get; set; }
    }
}
