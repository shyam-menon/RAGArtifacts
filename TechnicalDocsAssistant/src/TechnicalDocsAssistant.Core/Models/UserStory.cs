using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Postgrest.Attributes;
using Postgrest.Models;

namespace TechnicalDocsAssistant.Core.Models
{
    [Table("user_stories")]
    public class UserStory : BaseModel
    {
        [PrimaryKey("id", false)]
        [Column("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [Column("title")]
        [Required]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        [Required]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [Column("actors")]
        [JsonPropertyName("actors")]
        public List<string> Actors { get; set; } = new();

        [Column("preconditions")]
        [JsonPropertyName("preconditions")]
        public List<string> Preconditions { get; set; } = new();

        [Column("postconditions")]
        [JsonPropertyName("postconditions")]
        public List<string> Postconditions { get; set; } = new();

        [Column("main_flow")]
        [Required]
        [JsonPropertyName("mainFlow")]
        public List<string> MainFlow { get; set; } = new();

        [Column("alternative_flows")]
        [JsonPropertyName("alternativeFlows")]
        public List<string> AlternativeFlows { get; set; } = new();

        [Column("business_rules")]
        [JsonPropertyName("businessRules")]
        public List<string> BusinessRules { get; set; } = new();

        [Column("data_requirements")]
        [JsonPropertyName("dataRequirements")]
        public List<string> DataRequirements { get; set; } = new();

        [Column("non_functional_requirements")]
        [JsonPropertyName("nonFunctionalRequirements")]
        public List<string> NonFunctionalRequirements { get; set; } = new();

        [Column("assumptions")]
        [JsonPropertyName("assumptions")]
        public List<string> Assumptions { get; set; } = new();

        [Column("created_at")]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public bool IsDeleted { get; set; }

        [JsonIgnore]
        public bool IsNew { get; set; }
    }
}
