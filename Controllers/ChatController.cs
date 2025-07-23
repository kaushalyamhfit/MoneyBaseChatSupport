using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MoneyBaseChatSupport.Services;

namespace MoneyBaseChatSupport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatQueueService _chatQueueService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatQueueService chatQueueService, ILogger<ChatController> logger)
        {
            _chatQueueService = chatQueueService;
            _logger = logger;
        }

        [HttpPost("request")]
        public IActionResult RequestChat()
        {
            _logger.LogInformation("Received a chat request.");
            var chat = _chatQueueService.CreateChat();
            if (chat == null)
            {
                _logger.LogWarning("Chat creation failed: queue full or unavailable.");
                return StatusCode(503, "Chat queue is full or support is unavailable at this time.");
            }

            _chatQueueService.AssignChats();
            _logger.LogInformation("Chat {ChatId} processed successfully.", chat.Id);

            return Ok(new
            {
                ChatId = chat.Id,
                AssignedAgent = chat.AssignedAgent?.Name ?? "Pending Assignment"
            });
        }

        [HttpGet("status/{chatId}")]
        public IActionResult GetChatStatus(Guid chatId)
        {
            _logger.LogInformation("Status request for chat {ChatId}.", chatId);
            var chat = _chatQueueService.AllChats.FirstOrDefault(c => c.Id == chatId);
            if (chat == null)
            {
                _logger.LogWarning("Status check failed: chat {ChatId} not found.", chatId);
                return NotFound("Chat session not found.");
            }

            _logger.LogInformation("Returning status for chat {ChatId}: Active = {IsActive}, MissedPolls = {MissedPolls}.",
                chatId, chat.IsActive, chat.MissedPolls);

            return Ok(new
            {
                ChatId = chat.Id,
                IsActive = chat.IsActive,
                MissedPolls = chat.MissedPolls,
                AssignedAgent = chat.AssignedAgent?.Name ?? "Unassigned",
                LastPolled = chat.LastPollTime.ToString("HH:mm:ss")
            });
        }

        [HttpPost("poll/{chatId}")]
        public IActionResult Poll(Guid chatId)
        {
            _logger.LogInformation("Poll request received for chat {ChatId}.", chatId);
            var chat = _chatQueueService.AllChats.FirstOrDefault(c => c.Id == chatId);
            if (chat == null) {
                _logger.LogWarning("Poll failed: chat {ChatId} not found.", chatId);
                return NotFound("Chat session not found."); 
            }
            if (!chat.IsActive)
            {
                _logger.LogWarning("Poll failed: chat {ChatId} is inactive.", chatId);
                return BadRequest("Chat session is inactive.");
            }

            chat.LastPollTime = DateTime.UtcNow;
            chat.MissedPolls = 0;

            _logger.LogInformation("Poll acknowledged for chat {ChatId}.", chatId);
            return Ok("Chat is active.");
        }
    }
}
