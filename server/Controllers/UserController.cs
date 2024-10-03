using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using server.internals.helpers;
using System.ComponentModel.DataAnnotations;
using server.internals.dbServices;
using server.internals.dbMigrations.tables;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.Tokens;

namespace server.Controllers
{
    [ApiController]
    [Route("/")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly ProfilePictureService _profilePictureService;
        class SuccessfulLoginResponse() 
        {
            public string token { get; set; }
            public long expiresAt { get; set; }
        }
        class PartiallySuccessfulLoginResponse() : SuccessfulLoginResponse
        {
            public string? errorMessage { get; set; }
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
        public UserController(UserService userService, TokenService tokenService, ProfilePictureService profilePictureService) 
        {
            _userService = userService;
            _tokenService = tokenService;
            _profilePictureService = profilePictureService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var errorResponse = new ErrorResponse();
            if (!ModelState.IsValid) 
            {
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty fields 'username', 'email', 'password', 'username' and possibly 'base64encodedimage'";
                return BadRequest(errorResponse);
            }
            var newUser = new User();
            newUser.UserStatus = (int)UserStatuses.ONLINE;
            newUser.Username = model.username; 
            newUser.Email = model.email;
            newUser.Salt = BCrypt.Net.BCrypt.GenerateSalt();
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(model.password, newUser.Salt);
            var errorUser = await _userService.AddUser(newUser);
            if (errorUser != ErrorCodes.NO_ERROR)
            {
                errorResponse.errorMessage = "Failed to write user data to database";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            var (token, errorToken) = await _tokenService.GenerateTokenForAUser(newUser);
            if (errorToken != ErrorCodes.NO_ERROR)
            {
                errorResponse.errorMessage = "Failed to generate token";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            if (!model.base64encodedimage.IsNullOrEmpty()) 
            {
                var errorImage = await _profilePictureService.SetProfilePictureForUser(newUser, model.base64encodedimage);
                if (errorImage != ErrorCodes.NO_ERROR)
                {
                    var response = new PartiallySuccessfulLoginResponse { expiresAt = token.ExpiresAt, token = token.JWToken };
                    response.errorMessage = "Failed to set profile picture";
                    return CreatedAtAction(nameof(Register), response);
                }
            }
            return CreatedAtAction(nameof(Register), new SuccessfulLoginResponse { expiresAt = token.ExpiresAt, token = token.JWToken });
        }
        public class LoginByUsernameModel()
        {
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string username { get; set; }
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string password { get; set; }
        }

        [HttpPost("login-username")]
        public async Task<IActionResult> LoginByUsername([FromBody] LoginByUsernameModel model)
        {
            var errorResponse = new ErrorResponse();
            if (!ModelState.IsValid)
            {
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty fields 'username' and 'password'";
                return BadRequest(errorResponse);
            }
            var (user, error) = await _userService.GetUserByUsernameAndPassword(model.username, model.password);
            if (error != ErrorCodes.NO_ERROR)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            if (user == null) 
            {
                errorResponse.errorMessage = "Failed to find user with given username and password";
                return NotFound(errorResponse);
            }
            return Ok(user);
        }
        public class LoginByEmailModel()
        {
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string email { get; set; }
            [StringLength(int.MaxValue, MinimumLength = 1)]
            public string password { get; set; }
        }

        [HttpPost("login-email")]
        public async Task<IActionResult> LoginByEmail([FromBody] LoginByEmailModel model)
        {
            var errorResponse = new ErrorResponse();
            if (!ModelState.IsValid)
            {
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty fields 'email' and 'password'";
                return BadRequest(errorResponse);
            }
            var (user, error) = await _userService.GetUserByEmailAndPassword(model.email, model.password);
            if (error != ErrorCodes.NO_ERROR)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            if (user == null)
            {
                errorResponse.errorMessage = "Failed to find user with given email and password";
                return NotFound(errorResponse);
            }
            return Ok(user);
        }
        public class ChangeProfilePictureModel()
        {
            public string base64encodedimage { get; set; }
        }
        [HttpPost("change-profile-picture")]
        public async Task<IActionResult> ChangeProfilePicture([FromBody] ChangeProfilePictureModel model)
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
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty field 'base64encodedimage'";
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
            var errorImage = await _profilePictureService.SetProfilePictureForUser(user, model.base64encodedimage);
            if (errorImage != ErrorCodes.NO_ERROR)
            {
                errorResponse.errorMessage = "Failed to set profile picture";
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            return Ok();
        }
        class UserInformation
        {
            public string username;
            public string email;
            public string base64encodedimage;
        }
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var errorResponse = new ErrorResponse();
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (token.IsNullOrEmpty())
            {
                errorResponse.errorMessage = "'Authorization' header was not provided";
                return Unauthorized(errorResponse);
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
            var (profilePicture, errorProfilePicture) = await _profilePictureService.GetProfilePictureOfUser(user);
            var infromation = new UserInformation { username = user.Username, email = user.Email};
            if (errorProfilePicture != ErrorCodes.NO_ERROR || user == null)
            {
                infromation.base64encodedimage = "";
            } else
            {
                infromation.base64encodedimage = profilePicture.Base64EncodedImage;
            }
            return Ok(infromation);
        }
    }
}
