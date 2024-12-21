using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Core.Models.Chat;

namespace TechnicalDocsAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("initialize")]
        public async Task<IActionResult> Initialize()
        {
            await _chatService.InitializeVectorStoreAsync();
            return Ok(new { message = "Vector store initialized successfully" });
        }

        [HttpPost("query")]
        public async Task<ActionResult<ChatResponse>> Query([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "Query cannot be empty" });
            }

            var response = await _chatService.QueryAsync(request);
            return Ok(response);
        }
    }
}
