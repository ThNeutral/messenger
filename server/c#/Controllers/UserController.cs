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
    public class UserController : AbstractController
    {
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly ProfilePictureService _profilePictureService;
        public struct SuccessfulLoginResponse() 
        {
            public string token { get; set; }
            public long expiresAt { get; set; }
        }
        struct PartiallySuccessfulLoginResponse()
        {
            public string token { get; set; }
            public long expiresAt { get; set; }
            public string? errorMessage { get; set; }
        }
        public struct RegisterModel()
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
                errorResponse.errorMessage = "Incorrect request body. Expected non-empty fields 'username', 'email', 'password' and possibly 'base64encodedimage'";
                return BadRequest(errorResponse);
            }
            var newUser = new User();
            newUser.UserID = CustomRandom.GenerateRandomULong();
            newUser.Username = model.username; 
            newUser.Email = model.email;
            newUser.Salt = BCrypt.Net.BCrypt.GenerateSalt();
            newUser.Password = BCrypt.Net.BCrypt.HashPassword(model.password, newUser.Salt);
            var errorUser = await _userService.AddUser(newUser);
            if (errorUser == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to write user data to database";
                return InternalServerError(errorResponse);
            }
            if (errorUser == ErrorCodes.UNIQUE_CONSTRAINT_VIOLATION)
            {
                errorResponse.errorMessage = "User with this 'username' or 'email' already exists";
            }
            var (token, errorToken) = await _tokenService.GenerateTokenForAUser(newUser);
            if (errorToken == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to generate token";
                return InternalServerError(errorResponse);
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
        public struct LoginByUsernameModel()
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
            if (error == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (error == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY) 
            {
                errorResponse.errorMessage = "Failed to find user with given username and password";
                return NotFound(errorResponse);
            }
            var (token, errorToken) = await _tokenService.GetTokenOfAUser(user);
            if (errorToken == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorToken == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find token with given user";
                return NotFound(errorResponse);
            }
            return Ok(new SuccessfulLoginResponse { expiresAt = token.ExpiresAt, token = token.JWToken }) ;
        }
        public struct LoginByEmailModel()
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
            if (error == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (error == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find user with given email and password";
                return NotFound(errorResponse);
            }
            var (token, errorToken) = await _tokenService.GetTokenOfAUser(user);
            if (errorToken == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to connect to database";
                return InternalServerError(errorResponse);
            }
            if (errorToken == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
            {
                errorResponse.errorMessage = "Failed to find token with given user";
                return NotFound(errorResponse);
            }
            return Ok(new SuccessfulLoginResponse { expiresAt = token.ExpiresAt, token = token.JWToken });
        }
        public struct ChangeProfilePictureModel()
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
            var errorImage = await _profilePictureService.SetProfilePictureForUser(user, model.base64encodedimage);
            if (errorImage == ErrorCodes.DB_TRANSACTION_FAILED)
            {
                errorResponse.errorMessage = "Failed to set profile picture";
                return InternalServerError(errorResponse);
            }
            return Ok();
        }
        public struct UserInformation
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
            var (profilePicture, errorProfilePicture) = await _profilePictureService.GetProfilePictureOfUser(user);
            var infromation = new UserInformation { username = user.Username, email = user.Email};
            if (errorProfilePicture == ErrorCodes.DB_TRANSACTION_FAILED || errorProfilePicture == ErrorCodes.FAILED_TO_FIND_GIVEN_ENTRY)
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
