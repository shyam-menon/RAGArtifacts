using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Core.Interfaces;
using System.Text.Json;

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public class UserStoryAgent
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IAssetService _assetService;

        public UserStoryAgent(
            IChatCompletionService chatCompletionService,
            IAssetService assetService)
        {
            _chatCompletionService = chatCompletionService;
            _assetService = assetService;
        }

        public async Task<string> GenerateUserStory(string input, float[] queryVector)
        {
            // Search for relevant context using the existing query vector
            var similarityThreshold = 0.3f;
            var limit = 3;
            var relevantDocs = await _assetService.GetSimilarAssetsAsync(queryVector, limit, similarityThreshold);

            var contextBuilder = new StringBuilder();
            foreach (var doc in relevantDocs)
            {
                contextBuilder.AppendLine(doc.MarkdownContent);
            }

            var prompt = $@"You are a technical documentation expert. Create a detailed user story based on the input and reference information.
Your response must be a valid JSON object with the following structure. Do not include any other text or formatting outside of this JSON structure:

{{
    ""title"": ""A clear, concise title describing the feature"",
    ""description"": ""A detailed description in the format: As a [role], I want [feature] so that [benefit]"",
    ""actors"": [
        ""List of actors/roles involved""
    ],
    ""preconditions"": [
        ""List of conditions that must be true before the story can be implemented""
    ],
    ""postconditions"": [
        ""List of conditions that must be true after the story is implemented""
    ],
    ""mainFlow"": [
        ""Step-by-step list of the main success scenario""
    ],
    ""alternativeFlows"": [
        ""List of alternative paths or exception scenarios""
    ],
    ""businessRules"": [
        ""List of business rules and constraints""
    ],
    ""dataRequirements"": [
        ""List of data elements and their characteristics""
    ],
    ""nonFunctionalRequirements"": [
        ""List of performance, security, and other non-functional requirements""
    ],
    ""assumptions"": [
        ""List of assumptions made in this user story""
    ]
}}

Reference Information:
{contextBuilder}

Input:
{input}

Remember: Return ONLY the JSON object, no additional text or formatting.";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(prompt);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);
            
            try
            {
                // Try to parse the response to ensure it's valid JSON
                using (JsonDocument.Parse(response.Content))
                {
                    return response.Content; // Return the valid JSON string
                }
            }
            catch (JsonException)
            {
                Console.WriteLine($"Failed to parse JSON response: {response.Content}");
                // If JSON parsing fails, return a formatted error message
                return JsonSerializer.Serialize(new
                {
                    title = "Error Generating User Story",
                    description = "Failed to generate a properly formatted user story. Please try again.",
                    actors = new[] { "Error" },
                    preconditions = new[] { "Error generating user story" },
                    postconditions = new[] { "Error generating user story" },
                    mainFlow = new[] { "Error generating user story" },
                    alternativeFlows = new[] { "Error generating user story" },
                    businessRules = new[] { "Error generating user story" },
                    dataRequirements = new[] { "Error generating user story" },
                    nonFunctionalRequirements = new[] { "Error generating user story" },
                    assumptions = new[] { "Error generating user story" }
                });
            }
        }
    }
}
