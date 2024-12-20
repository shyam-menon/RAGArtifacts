using System;

namespace TechnicalDocsAssistant.Core.Models
{
    public class TechnicalArtifact
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Type { get; set; }  // flowchart, sequence, testcase
        public string Content { get; set; }  // PlantUML or markdown content
        public string UserStoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string GeneratedBy { get; set; }  // Plugin or model that generated this artifact
        public string Version { get; set; } = "1.0";  // Version of the artifact
    }

    public static class ArtifactType
    {
        public const string Flowchart = "flowchart";
        public const string SequenceDiagram = "sequence";
        public const string TestCase = "testcase";
    }
}
