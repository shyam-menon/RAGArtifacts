using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPost]
        public async Task<IActionResult> CreateUserStory([FromBody] UserStory userStory)
        {
            var result = await _userStoryService.CreateUserStoryAsync(userStory);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserStory(string id, [FromBody] UserStory userStory)
        {
            if (id != userStory.Id)
            {
                return BadRequest("ID mismatch");
            }

            var result = await _userStoryService.UpdateUserStoryAsync(userStory);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserStory(string id)
        {
            var userStory = await _userStoryService.GetUserStoryByIdAsync(id);
            if (userStory == null)
            {
                return NotFound();
            }
            return Ok(userStory);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserStories()
        {
            var userStories = await _userStoryService.GetAllUserStoriesAsync();
            return Ok(userStories);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserStory(string id)
        {
            await _userStoryService.DeleteUserStoryAsync(id);
            return NoContent();
        }
    }
}
