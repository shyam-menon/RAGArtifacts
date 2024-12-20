using System;
using System.ComponentModel.DataAnnotations;

namespace TechnicalDocsAssistant.Core.Models
{
    public class TechnicalArtifact
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string Type { get; set; } = string.Empty;  // flowchart, sequence, testcase

        [Required]
        public string Content { get; set; } = string.Empty;  // PlantUML or markdown content

        [Required]
        public string UserStoryId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string GeneratedBy { get; set; } = string.Empty;  // Plugin or model that generated this artifact

        public string Version { get; set; } = "1.0";  // Version of the artifact
    }

    public static class ArtifactType
    {
        public const string Flowchart = "flowchart";
        public const string SequenceDiagram = "sequence";
        public const string TestCase = "testcase";
    }
}
