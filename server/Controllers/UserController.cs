using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using server.internals.helpers;
using System.ComponentModel.DataAnnotations;
using server.internals.dbServices;
using server.internals.dbMigrations.tables;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace server.Controllers
{
    [ApiController]
    [Route("/")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        class RegisterResponse() 
        {
            public int id { get; set; }
        }
        public class RegisterModel()
        {
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string username { get; set; }
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string email { get; set; }
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string password { get; set; }
            public string? base64encodedimage { get; set; }
        }
        public UserController(UserService userService) 
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var errorResponse = new ErrorResponse();
            if (!ModelState.IsValid) 
            {
                errorResponse.errorMessage = "Incorrect request form. Expected non-empty fields 'username', 'email', 'password', 'username' and possibly 'base64encodedimage'";
                return BadRequest(errorResponse);
            }
            var newUser = new User();
            newUser.UserStatus = (int)UserStatuses.ONLINE;
            newUser.Username = model.username; 
            newUser.Email = model.email;
            newUser.Salt = BCrypt.Net.BCrypt.GenerateSalt();
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(model.password, newUser.Salt);
            var error = await _userService.AddUserAsync(newUser);
            if (error != ErrorCodes.NO_ERROR)
            {
                errorResponse.errorMessage = "Failed to write user data to database";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            return CreatedAtAction(nameof(Register), null);
        }
    }
}
