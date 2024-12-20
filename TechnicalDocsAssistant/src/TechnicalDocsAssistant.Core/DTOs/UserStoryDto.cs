using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TechnicalDocsAssistant.Core.Models;

namespace TechnicalDocsAssistant.Core.DTOs
{
    public class UserStoryDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("actors")]
        public List<string> Actors { get; set; } = new();

        [JsonPropertyName("preconditions")]
        public List<string> Preconditions { get; set; } = new();

        [JsonPropertyName("postconditions")]
        public List<string> Postconditions { get; set; } = new();

        [JsonPropertyName("mainFlow")]
        public List<string> MainFlow { get; set; } = new();

        [JsonPropertyName("alternativeFlows")]
        public List<string> AlternativeFlows { get; set; } = new();

        [JsonPropertyName("businessRules")]
        public List<string> BusinessRules { get; set; } = new();

        [JsonPropertyName("dataRequirements")]
        public List<string> DataRequirements { get; set; } = new();

        [JsonPropertyName("nonFunctionalRequirements")]
        public List<string> NonFunctionalRequirements { get; set; } = new();

        [JsonPropertyName("assumptions")]
        public List<string> Assumptions { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        public static UserStoryDto FromUserStory(UserStory userStory)
        {
            return new UserStoryDto
            {
                Id = userStory.Id,
                Title = userStory.Title,
                Description = userStory.Description,
                Actors = userStory.Actors,
                Preconditions = userStory.Preconditions,
                Postconditions = userStory.Postconditions,
                MainFlow = userStory.MainFlow,
                AlternativeFlows = userStory.AlternativeFlows,
                BusinessRules = userStory.BusinessRules,
                DataRequirements = userStory.DataRequirements,
                NonFunctionalRequirements = userStory.NonFunctionalRequirements,
                Assumptions = userStory.Assumptions,
                CreatedAt = userStory.CreatedAt,
                UpdatedAt = userStory.UpdatedAt
            };
        }
    }
}
