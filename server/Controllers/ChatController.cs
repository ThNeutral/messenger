using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using server.internals.dbServices;
using server.internals.helpers;
using System.ComponentModel.DataAnnotations;

namespace server.Controllers
{
    [ApiController]
    [Route("/")]
    public class ChatController : ControllerBase
    {
        public UserService _userService;
        public ChatService _chatService;
        public ChatController(UserService userService, ChatService chatService) 
        {
            _userService = userService;    
            _chatService = chatService;
        }
        public class CreateChatModel
        {
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string chat_name;
        }
        [HttpPost("create-chat")]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatModel model)
        {
            var errorResponse = new ErrorResponse();
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (token.IsNullOrEmpty())
            {
                errorResponse.errorMessage = "'Authorization' header was not provided";
                return Unauthorized(errorResponse);
            }
            if (!ModelState.IsValid)
            {
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty field 'chat_name'";
                return BadRequest(errorResponse);
            }
            var (user, errorUser) = await _userService.GetUserByToken(token);
            if (errorUser != ErrorCodes.NO_ERROR)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            if (user == null)
            {
                errorResponse.errorMessage = "Did not find user with such token";
                return NotFound(errorResponse);
            }
            var (chat, errorCreateChat) = await _chatService.CreateChat(user, model.chat_name);
            if (errorCreateChat != ErrorCodes.NO_ERROR)
            {
                errorResponse.errorMessage = "Failed to create chat";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            return CreatedAtAction(nameof(CreateChat), new { chat_id = chat.ChatID });
        }
        public class AddUsersToChatModel
        {
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string users;
        }
        [HttpPost("add-users-to-chat")]
        public async Task<IActionResult> AddUserToChat([FromBody] AddUsersToChatModel model)
        {
            Console.WriteLine(model.users);
            var errorResponse = new ErrorResponse();
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (token.IsNullOrEmpty())
            {
                errorResponse.errorMessage = "'Authorization' header was not provided";
                return Unauthorized(errorResponse);
            }
            if (!ModelState.IsValid)
            {
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty field 'users'";
                return BadRequest(errorResponse);
            }
            return Ok(model);
        }
    }
}
