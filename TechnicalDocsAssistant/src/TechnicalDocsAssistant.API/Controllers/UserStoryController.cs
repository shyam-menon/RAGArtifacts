using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechnicalDocsAssistant.Core.DTOs;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Core.Models;

namespace TechnicalDocsAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserStoryController : ControllerBase
    {
        private readonly IUserStoryService _userStoryService;

        public UserStoryController(IUserStoryService userStoryService)
        {
            _userStoryService = userStoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserStories()
        {
            var userStories = await _userStoryService.GetAllUserStoriesAsync();
            return Ok(userStories.Select(MapToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserStory(string id)
        {
            var userStory = await _userStoryService.GetUserStoryByIdAsync(id);
            if (userStory == null)
            {
                return NotFound();
            }
            return Ok(MapToDto(userStory));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserStory([FromBody] UserStoryDto dto)
        {
            var userStory = MapToUserStory(dto);
            var result = await _userStoryService.CreateUserStoryAsync(userStory);
            return Ok(MapToDto(result));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserStory(string id, [FromBody] UserStoryDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var userStory = MapToUserStory(dto);
            var result = await _userStoryService.UpdateUserStoryAsync(userStory);
            return Ok(MapToDto(result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserStory(string id)
        {
            await _userStoryService.DeleteUserStoryAsync(id);
            return NoContent();
        }

        private static UserStoryDto MapToDto(UserStory userStory)
        {
            return UserStoryDto.FromUserStory(userStory);
        }

        private static UserStory MapToUserStory(UserStoryDto dto)
        {
            return new UserStory
            {
                Id = dto.Id,
                Title = dto.Title,
                Description = dto.Description,
                Actors = dto.Actors,
                Preconditions = dto.Preconditions,
                Postconditions = dto.Postconditions,
                MainFlow = dto.MainFlow,
                AlternativeFlows = dto.AlternativeFlows,
                BusinessRules = dto.BusinessRules,
                DataRequirements = dto.DataRequirements,
                NonFunctionalRequirements = dto.NonFunctionalRequirements,
                Assumptions = dto.Assumptions,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }
    }
}
