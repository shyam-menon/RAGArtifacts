using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using System.Text.Json;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.SKPlugins;

namespace TechnicalDocsAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtifactController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly ArtifactGenerationPlugin _artifactPlugin;

        public ArtifactController(Kernel kernel)
        {
            _kernel = kernel;
            _artifactPlugin = new ArtifactGenerationPlugin(kernel);
        }

        [HttpPost("generate")]
        public async Task<ActionResult<TechnicalArtifact>> GenerateArtifact([FromBody] GenerateArtifactRequest request)
        {
            try
            {
                var userStoryJson = JsonSerializer.Serialize(request.UserStory);
                string content;

                switch (request.ArtifactType.ToLower())
                {
                    case ArtifactType.Flowchart:
                        content = await _artifactPlugin.GenerateFlowchart(userStoryJson);
                        break;
                    case ArtifactType.SequenceDiagram:
                        content = await _artifactPlugin.GenerateSequenceDiagram(userStoryJson);
                        break;
                    case ArtifactType.TestCase:
                        content = await _artifactPlugin.GenerateTestCases(userStoryJson);
                        break;
                    default:
                        return BadRequest($"Unsupported artifact type: {request.ArtifactType}");
                }

                var artifact = new TechnicalArtifact
                {
                    Type = request.ArtifactType,
                    Content = content,
                    UserStoryId = request.UserStory.Id,
                    GeneratedBy = "SemanticKernel"
                };

                return Ok(artifact);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating artifact: {ex.Message}");
            }
        }
    }

    public class GenerateArtifactRequest
    {
        public UserStory UserStory { get; set; }
        public string ArtifactType { get; set; }
    }
}
