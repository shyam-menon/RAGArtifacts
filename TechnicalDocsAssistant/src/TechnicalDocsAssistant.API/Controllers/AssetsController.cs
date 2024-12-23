#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0050

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using TechnicalDocsAssistant.Core.DTOs;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace TechnicalDocsAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly Kernel _kernel;

        public AssetsController(IAssetService assetService, Kernel kernel)
        {
            _assetService = assetService;
            _kernel = kernel;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssetResponseDTO>>> GetAllAssets()
        {
            var assets = await _assetService.GetAllAssetsAsync();
            return Ok(MapToResponseDTOs(assets));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AssetResponseDTO>> GetAsset(string id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
                return NotFound();

            return Ok(MapToResponseDTO(asset));
        }

        [HttpPost]
        public async Task<ActionResult<AssetResponseDTO>> CreateAsset([FromBody] CreateAssetDTO createDTO)
        {
            if (string.IsNullOrWhiteSpace(createDTO.Title))
            {
                return BadRequest("Title is required");
            }

            if (string.IsNullOrWhiteSpace(createDTO.MarkdownContent))
            {
                return BadRequest("MarkdownContent is required");
            }

            if (createDTO.Title.Length > 200)
            {
                return BadRequest("Title must not exceed 200 characters");
            }

            try
            {
                var asset = await _assetService.CreateAssetAsync(createDTO.Title, createDTO.MarkdownContent);
                return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, MapToResponseDTO(asset));
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to create asset: {ex.Message}");
            }
        }

        [HttpPost("markdown")]
        public async Task<ActionResult<AssetResponseDTO>> CreateAssetFromMarkdown([FromBody] string markdownContent)
        {
            try
            {
                // Extract the first heading as the title
                var lines = markdownContent.Split('\n');
                var title = "Untitled Document";
                
                // Look for the first # heading
                foreach (var line in lines)
                {
                    if (line.StartsWith("# "))
                    {
                        title = line.Substring(2).Trim();
                        break;
                    }
                }

                // If title is too long, truncate it
                if (title.Length > 200)
                {
                    title = title.Substring(0, 197) + "...";
                }

                var asset = await _assetService.CreateAssetAsync(title, markdownContent);
                return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, MapToResponseDTO(asset));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AssetResponseDTO>> UpdateAsset(string id, [FromBody] UpdateAssetDTO updateDTO)
        {
            try
            {
                var asset = await _assetService.UpdateAssetAsync(id, updateDTO.Title, updateDTO.MarkdownContent);
                return Ok(MapToResponseDTO(asset));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsset(string id)
        {
            try
            {
                await _assetService.DeleteAssetAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Query cannot be empty" });
            }

            // Generate embedding for the query
            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            var queryEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(query);
            var queryVector = queryEmbedding.ToArray();

            var assets = await _assetService.GetSimilarAssetsAsync(queryVector, limit);
            return Ok(assets);
        }

        [HttpPost("update-embeddings")]
        public async Task<IActionResult> UpdateEmbeddings()
        {
            await _assetService.UpdateEmbeddingsAsync();
            return Ok(new { message = "Embeddings updated successfully" });
        }

        private static AssetResponseDTO MapToResponseDTO(Asset asset)
        {
            return new AssetResponseDTO
            {
                Id = asset.Id,
                Title = asset.Title,
                MarkdownContent = asset.MarkdownContent,
                Created = asset.Created,
                Modified = asset.Modified
            };
        }

        private static IEnumerable<AssetResponseDTO> MapToResponseDTOs(IEnumerable<Asset> assets)
        {
            var dtos = new List<AssetResponseDTO>();
            foreach (var asset in assets)
            {
                dtos.Add(MapToResponseDTO(asset));
            }
            return dtos;
        }
    }
}
