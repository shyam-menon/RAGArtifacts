using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using TechnicalDocsAssistant.Core.DTOs;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.SKPlugins;
using TechnicalDocsAssistant.Core.Interfaces;

namespace TechnicalDocsAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtifactController : ControllerBase
    {
        private readonly Kernel _kernel;
        private readonly ArtifactGenerationPlugin _artifactPlugin;
        private readonly IUserStoryService _userStoryService;

        public ArtifactController(Kernel kernel, IUserStoryService userStoryService)
        {
            _kernel = kernel;
            _userStoryService = userStoryService;
            _artifactPlugin = new ArtifactGenerationPlugin(kernel);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateArtifact([FromBody] GenerateArtifactRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var userStory = await _userStoryService.GetUserStoryByIdAsync(request.UserStoryId);
                if (userStory == null)
                {
                    return NotFound($"User story with ID {request.UserStoryId} not found");
                }

                var userStoryDto = UserStoryDto.FromUserStory(userStory);
                var userStoryJson = JsonSerializer.Serialize(userStoryDto);
                string content;

                switch (request.ArtifactType.ToLower())
                {
                    case "flowchart":
                        content = await _artifactPlugin.GenerateFlowchart(userStoryJson);
                        break;
                    case "sequence":
                        content = await _artifactPlugin.GenerateSequenceDiagram(userStoryJson);
                        break;
                    case "testcases":
                        content = await _artifactPlugin.GenerateTestCases(userStoryJson);
                        break;
                    default:
                        return BadRequest($"Invalid artifact type: {request.ArtifactType}");
                }

                var artifact = new TechnicalArtifact
                {
                    Type = request.ArtifactType.ToLower(),
                    Content = content,
                    UserStoryId = request.UserStoryId
                };

                return Ok(artifact);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class GenerateArtifactRequest
    {
        [Required]
        public string UserStoryId { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(flowchart|sequence|testcases)$", ErrorMessage = "ArtifactType must be one of: flowchart, sequence, testcases")]
        public string ArtifactType { get; set; } = string.Empty;
    }
}
