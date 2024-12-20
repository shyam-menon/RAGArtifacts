using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TechnicalDocsAssistant.Core.Models
{
    public class UserStory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public List<string> Actors { get; set; } = new();

        public List<string> Preconditions { get; set; } = new();

        public List<string> Postconditions { get; set; } = new();

        [Required]
        public string MainFlow { get; set; } = string.Empty;

        public List<string> AlternativeFlows { get; set; } = new();

        public List<string> BusinessRules { get; set; } = new();

        public List<string> DataRequirements { get; set; } = new();

        public List<string> NonFunctionalRequirements { get; set; } = new();

        public List<string> Assumptions { get; set; } = new();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
