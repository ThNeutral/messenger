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
    public class ChatController : AbstractController
    {
        private readonly UserService _userService;
        private readonly ChatService _chatService;
        private readonly TokenService _tokenService;
        public ChatController(UserService userService, ChatService chatService)
        {
            _userService = userService;
            _chatService = chatService;
        }
        public struct CreateChatModel
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
            if (errorUser == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorUser == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Did not find user with such token";
                return NotFound(errorResponse);
            }
            var (chat, errorCreateChat) = await _chatService.CreateChat(user, model.chat_name);
            if (errorCreateChat == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to create chat";
                return InternalServerError(errorResponse);
            }
            return CreatedAtAction(nameof(CreateChat), new { chat_id = chat.ChatID, chat_name = model.chat_name });
        }
        public struct AddUsersToChatModel
        {
            public ulong chat_id;
            public ulong[] user_ids;
        }
        [HttpPost("add-users-to-chat")]
        public async Task<IActionResult> AddUserToChat([FromBody] AddUsersToChatModel model)
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
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty fields 'user_ids' and 'chat_id'";
                return BadRequest(errorResponse);
            }
            var (chat, errorChat) = await _chatService.GetChatByChatID(model.chat_id);
            if (errorChat == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorChat == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find chat with given chat_id";
                return NotFound(errorResponse);
            }
            var (userAdder, errorGetUserAdder) = await _userService.GetUserByToken(token);
            if (errorGetUserAdder == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorGetUserAdder == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find user with given token";
                return NotFound(errorResponse);
            }
            bool doesInclude = false;
            foreach (var ctu in chat.ChatsToUsers)
            {
                if (ctu.UserID == userAdder.UserID)
                {
                    doesInclude = true;
                    break;
                }
            }
            if (!doesInclude)
            {
                errorResponse.errorMessage = "User does not have rights to add users to given chat";
                return StatusCode(StatusCodes.Status403Forbidden, errorResponse);
            }
            var (users, errorGetUsers) = await _userService.GetUsersByIDs(model.user_ids);
            if (errorGetUsers == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorGetUsers == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "All of provided ids are incorrect";
                return NotFound(errorResponse);
            }
            if (errorGetUsers == ErrorCodes.FAILED_TO_FIND_SOME_ENTRIES)
            {
                errorResponse.errorMessage = "Some of provided ids are incorrect";
                return NotFound(errorResponse);
            }
            var errorAddUsers = await _chatService.AddUsersToExistingChat(chat, users);
            if (errorAddUsers == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            return Ok();
        }
        public struct GetMembersOfChatModel
        {
            public ulong chat_id;
        }
        [HttpPost("get-members-of-chat")]
        public async Task<IActionResult> GetMembersOfGivenChat([FromBody] GetMembersOfChatModel model)
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
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty field 'chat_id'";
                return BadRequest(errorResponse);
            }
            var (chat, errorChat) = await _chatService.GetChatByChatID(model.chat_id);
            if (errorChat == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorChat == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find chat with given chat_id";
                return NotFound(errorResponse);
            }
            var (userAdder, errorGetUserAdder) = await _userService.GetUserByToken(token);
            if (errorGetUserAdder == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorGetUserAdder == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find user with given token";
                return NotFound(errorResponse);
            }
            var userIDs = new List<ulong>();
            bool doesInclude = false;
            foreach (var ctu in chat.ChatsToUsers)
            {
                userIDs.Add(ctu.UserID);
                if (ctu.UserID == userAdder.UserID)
                {
                    doesInclude = true;
                }
            }
            if (!doesInclude)
            {
                errorResponse.errorMessage = "User does not have rights to add users to given chat";
                return InternalServerError(errorResponse);
            }
            return Ok(new { user_ids = userIDs });
        }
        public struct DeleteUsersFromChatModel
        {
            public ulong chat_id;
            public ulong[] user_ids;
        }
        [HttpPost("delete-members-from-chat")]
        public async Task<IActionResult> DeleteMembersFromChat([FromBody] DeleteUsersFromChatModel model)
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
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty fields 'chat_id' and 'user_ids'";
                return BadRequest(errorResponse);
            }
            var (chat, errorChat) = await _chatService.GetChatByChatID(model.chat_id);
            if (errorChat == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorChat == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find chat with given chat_id";
                return NotFound(errorResponse);
            }
            var (userAdder, errorGetUserAdder) = await _userService.GetUserByToken(token);
            if (errorGetUserAdder == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorGetUserAdder == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find user with given token";
                return NotFound(errorResponse);
            }
            var userIDs = new List<ulong>();
            bool doesInclude = false;
            foreach (var ctu in chat.ChatsToUsers)
            {
                userIDs.Add(ctu.UserID);
                if (ctu.UserID == userAdder.UserID)
                {
                    doesInclude = true;
                }
            }
            if (!doesInclude)
            {
                errorResponse.errorMessage = "User does not have rights to delete users from given chat";
                return Forbidden(errorResponse);
            }
            var (usersToDelete, errorFindUsersToDelete) = await _userService.GetUsersByIDs(userIDs.ToArray());
            if (errorFindUsersToDelete == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorFindUsersToDelete == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "All of provided ids are incorrect";
                return NotFound(errorResponse);
            }
            if (errorFindUsersToDelete == ErrorCodes.FAILED_TO_FIND_SOME_ENTRIES)
            {
                errorResponse.errorMessage = "Some of provided ids are incorrect";
                return NotFound(errorResponse);
            }
            var errorDeleteUsers = await _chatService.DeleteUsersFromChat(chat, usersToDelete.ToArray());
            if (errorDeleteUsers == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to delete users";
                return InternalServerError(errorResponse);
            }
            return Ok();
        }
        public struct DeleteChatModel
        {
            public ulong chat_id;
        }
        [HttpPost("delete-chat")]
        public async Task<IActionResult> DeleteChat([FromBody] DeleteChatModel model)
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
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty fields 'chat_id' and 'user_ids'";
                return BadRequest(errorResponse);
            }
            var (chat, errorChat) = await _chatService.GetChatByChatID(model.chat_id);
            if (errorChat == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            if (errorChat == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find chat with given chat_id";
                return NotFound(errorResponse);
            }
            var (userAdder, errorGetUserAdder) = await _userService.GetUserByToken(token);
            if (errorGetUserAdder == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorGetUserAdder == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find user with given token";
                return NotFound(errorResponse);
            }
            bool doesInclude = false;
            foreach (var ctu in chat.ChatsToUsers)
            {
                if (ctu.UserID == userAdder.UserID)
                {
                    doesInclude = true;
                }
            }
            if (!doesInclude)
            {
                errorResponse.errorMessage = "User does not have rights to delete users from given chat";
                return Forbidden(errorResponse);
            }
            var errorDeleteChat = await _chatService.DeleteChat(chat);
            if (errorDeleteChat == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to delete chat";
                return InternalServerError(errorResponse);
            }
            return Ok();
        }
        struct ChatData
        {
            public string name;
            public ulong id;
        }
        struct GetMyChatsResponse
        {
            public ChatData[] chats;
        }

        [HttpPost("get-my-chats")]
        public async Task<IActionResult> GetMyChats()
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
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty fields 'chat_id' and 'user_ids'";
                return BadRequest(errorResponse);
            }
            var (userAdder, errorGetUserAdder) = await _userService.GetUserByToken(token);
            if (errorGetUserAdder == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorGetUserAdder == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find user with given token";
                return NotFound(errorResponse);
            }
            var (chats, errorGetChats) = await _chatService.GetChatsOfUser(userAdder);
            if (errorGetChats == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            var chatDatas = new List<ChatData>();
            foreach (var chat in chats) 
            {
                chatDatas.Add(new ChatData { id = chat.ChatID, name = chat.ChatName });
            }
            return Ok(new GetMyChatsResponse { chats = chatDatas.ToArray() });
        }
    }
}
