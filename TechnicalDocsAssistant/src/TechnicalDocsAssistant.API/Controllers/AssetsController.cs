using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechnicalDocsAssistant.Core.DTOs;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Core.Models;

namespace TechnicalDocsAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
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
            try
            {
                var asset = await _assetService.CreateAssetAsync(createDTO.Title, createDTO.MarkdownContent);
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

        [HttpPost("search")]
        public async Task<ActionResult<IEnumerable<AssetResponseDTO>>> SearchAssets([FromBody] SearchAssetsDTO searchDTO)
        {
            try
            {
                var assets = await _assetService.SearchSimilarAssetsAsync(searchDTO.Query, searchDTO.Limit);
                return Ok(MapToResponseDTOs(assets));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
