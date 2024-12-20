using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Postgrest.Attributes;
using Postgrest.Models;

namespace TechnicalDocsAssistant.Core.Models
{
    [Table("user_stories")]
    public class UserStory : BaseModel
    {
        [PrimaryKey("id")]
        [Column("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Column("title")]
        [Required]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        [Required]
        public string Description { get; set; } = string.Empty;

        [Column("actors")]
        public List<string> Actors { get; set; } = new();

        [Column("preconditions")]
        public List<string> Preconditions { get; set; } = new();

        [Column("postconditions")]
        public List<string> Postconditions { get; set; } = new();

        [Column("main_flow")]
        [Required]
        public List<string> MainFlow { get; set; } = new();

        [Column("alternative_flows")]
        public List<string> AlternativeFlows { get; set; } = new();

        [Column("business_rules")]
        public List<string> BusinessRules { get; set; } = new();

        [Column("data_requirements")]
        public List<string> DataRequirements { get; set; } = new();

        [Column("non_functional_requirements")]
        public List<string> NonFunctionalRequirements { get; set; } = new();

        [Column("assumptions")]
        public List<string> Assumptions { get; set; } = new();

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
