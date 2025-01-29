using System.Net;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using MagicVilla.Villa.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla.Villa.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [ApiVersionNeutral]
    public class AuthenticationController : ControllerBase
    {
        protected ApiResponse _apiResponse;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly double _accessTokenExpiry;
        private readonly double _refreshTokenExpiry;

        public AuthenticationController(
            IUserRepository userRepository, 
            IAuthenticationService authenticationService,
            IConfiguration configuration
        )
        {
            _userRepository = userRepository;
            this._apiResponse = new();
            _authenticationService = authenticationService;
            var accessTokenExpiryStr = configuration.GetValue<string>("ApiSettings:AccessTokenExpiryMinutes") ?? throw new InvalidOperationException("The 'ApiSettings:AccessTokenExpiryMinutes' configuration value is missing or null.");
            _accessTokenExpiry = Double.Parse(accessTokenExpiryStr);
            var refreshTokenExpiryStr = configuration.GetValue<string>("ApiSettings:RefreshTokenExpiryMinutes") ?? throw new InvalidOperationException("The 'ApiSettings:RefreshTokenExpiryMinutes' configuration value is missing or null.");
            _refreshTokenExpiry = Double.Parse(refreshTokenExpiryStr);
        }

        [HttpGet("error")]
        public async Task<IActionResult> Error()
        {
            throw new FileNotFoundException();
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse = await _authenticationService.Login(model);
            if (loginResponse.User == null || string.IsNullOrWhiteSpace(loginResponse.AccessToken))
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    "Username or password is incorrect"
                };
                return BadRequest(_apiResponse);
            }

            // Add the token as a secure HttpOnly cookie
            HttpContext.Response.Cookies.Append(
                "jwt",
                loginResponse.AccessToken,   
                new CookieOptions
                {
                    // Ensures the cookie is accessible only by the server, preventing JavaScript access to the cookie (protects against XSS attacks).
                    HttpOnly = true,
                    // Indicates that the cookie should not require HTTPS for transmission. 
                    // Set to `false` during development but should always be `true` in production to ensure cookies are sent only over secure HTTPS connections.  
                    Secure = false, // set to true in production to avoid Man-in-the-middle attacks
                    // Allows the cookie to be sent with cross-site requests, which is necessary in some scenarios (e.g., if the app interacts with APIs or resources hosted on different domains). 
                    // This setting is typically used for development but increases the risk of CSRF attacks in production unless proper safeguards (like anti-CSRF tokens) are implemented.
                    SameSite = SameSiteMode.None,   // set to Strict in production to avoid CSRF attacks
                    Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiry)
                }
            );

            // Add the token as a secure HttpOnly cookie
            HttpContext.Response.Cookies.Append(
                "refresh",
                loginResponse.RefreshAccessToken,   
                new CookieOptions
                {
                    // Ensures the cookie is accessible only by the server, preventing JavaScript access to the cookie (protects against XSS attacks).
                    HttpOnly = true,
                    // Indicates that the cookie should not require HTTPS for transmission. 
                    // Set to `false` during development but should always be `true` in production to ensure cookies are sent only over secure HTTPS connections.  
                    Secure = false, // set to true in production to avoid Man-in-the-middle attacks
                    // Allows the cookie to be sent with cross-site requests, which is necessary in some scenarios (e.g., if the app interacts with APIs or resources hosted on different domains). 
                    // This setting is typically used for development but increases the risk of CSRF attacks in production unless proper safeguards (like anti-CSRF tokens) are implemented.
                    SameSite = SameSiteMode.None,   // set to Strict in production to avoid CSRF attacks
                    Path = "/api/auth",
                    Expires = DateTime.UtcNow.AddMinutes(_refreshTokenExpiry)
                }
            );
            _apiResponse.StatusCode = HttpStatusCode.OK;
            _apiResponse.IsSuccess = true;
            _apiResponse.Result = loginResponse;
            return Ok(_apiResponse);
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {
            bool ifUserNameUnique = _userRepository.IsUniqueUser(model.UserName);
            if (!ifUserNameUnique)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    "Username already exists"
                };
                return BadRequest(_apiResponse);
            }

            var user = await _authenticationService.Register(model);
            if (user == null)
            {
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>()
                {
                    "Error while registering"
                };
                return BadRequest(_apiResponse);
            }

            _apiResponse.StatusCode = HttpStatusCode.OK;
            _apiResponse.IsSuccess = true;
            return Ok(_apiResponse);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNewTokenFromRefreshToken()
        {
            var refreshToken = HttpContext.Request.Cookies["refresh"];
            if (!String.IsNullOrWhiteSpace(refreshToken))
            {
                var tokenDtoResponse = await _authenticationService.RefreshAccessToken(refreshToken);
                if (tokenDtoResponse == null || string.IsNullOrWhiteSpace(tokenDtoResponse.AccessToken))
                {
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages = new List<string>
                    {
                        "Token invalid"
                    };
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_apiResponse);
                }

            // Add the token as a secure HttpOnly cookie
            HttpContext.Response.Cookies.Append(
                "jwt",
                tokenDtoResponse.AccessToken,   
                new CookieOptions
                {
                    // Ensures the cookie is accessible only by the server, preventing JavaScript access to the cookie (protects against XSS attacks).
                    HttpOnly = true,
                    // Indicates that the cookie should not require HTTPS for transmission. 
                    // Set to `false` during development but should always be `true` in production to ensure cookies are sent only over secure HTTPS connections.  
                    Secure = false, // set to true in production to avoid Man-in-the-middle attacks
                    // Allows the cookie to be sent with cross-site requests, which is necessary in some scenarios (e.g., if the app interacts with APIs or resources hosted on different domains). 
                    // This setting is typically used for development but increases the risk of CSRF attacks in production unless proper safeguards (like anti-CSRF tokens) are implemented.
                    SameSite = SameSiteMode.None,   // set to Strict in production to avoid CSRF attacks
                    Expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiry)
                }
            );

            // Add the token as a secure HttpOnly cookie
            HttpContext.Response.Cookies.Append(
                "refresh",
                tokenDtoResponse.RefreshToken,   
                new CookieOptions
                {
                    // Ensures the cookie is accessible only by the server, preventing JavaScript access to the cookie (protects against XSS attacks).
                    HttpOnly = true,
                    // Indicates that the cookie should not require HTTPS for transmission. 
                    // Set to `false` during development but should always be `true` in production to ensure cookies are sent only over secure HTTPS connections.  
                    Secure = false, // set to true in production to avoid Man-in-the-middle attacks
                    // Allows the cookie to be sent with cross-site requests, which is necessary in some scenarios (e.g., if the app interacts with APIs or resources hosted on different domains). 
                    // This setting is typically used for development but increases the risk of CSRF attacks in production unless proper safeguards (like anti-CSRF tokens) are implemented.
                    SameSite = SameSiteMode.None,   // set to Strict in production to avoid CSRF attacks
                    Path = "/api/auth",
                    Expires = DateTime.UtcNow.AddMinutes(_refreshTokenExpiry)
                }
            );
                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = tokenDtoResponse;
                return Ok(_apiResponse);
            }
            else
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>
                {
                    "Invalid Input"
                };
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_apiResponse);
            }
        }

        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RevokeRefreshToken()
        {
            var refreshToken = HttpContext.Request.Cookies["refresh"];
            if (!String.IsNullOrWhiteSpace(refreshToken))
            {
                await _authenticationService.RevokeRefreshToken(refreshToken);

                // Clear the Access Token cookie
                HttpContext.Response.Cookies.Append(
                    "jwt",
                    string.Empty, // Empty value
                    new CookieOptions
                    {
                        // Ensures the cookie is accessible only by the server, preventing JavaScript access to the cookie (protects against XSS attacks).
                        HttpOnly = true,
                        // Indicates that the cookie should not require HTTPS for transmission. 
                        // Set to `false` during development but should always be `true` in production to ensure cookies are sent only over secure HTTPS connections.  
                        Secure = false, // set to true in production to avoid Man-in-the-middle attacks
                        // Allows the cookie to be sent with cross-site requests, which is necessary in some scenarios (e.g., if the app interacts with APIs or resources hosted on different domains). 
                        // This setting is typically used for development but increases the risk of CSRF attacks in production unless proper safeguards (like anti-CSRF tokens) are implemented.
                        SameSite = SameSiteMode.None,   // set to Strict in production to avoid CSRF attacks
                        Expires = DateTime.UtcNow.AddDays(-1), // Expire immediately
                    }
                );

                // Clear the Refresh Token cookie
                HttpContext.Response.Cookies.Append(
                    "refresh",
                    string.Empty, // Empty value
                    new CookieOptions
                    {
                        // Ensures the cookie is accessible only by the server, preventing JavaScript access to the cookie (protects against XSS attacks).
                        HttpOnly = true,
                        // Indicates that the cookie should not require HTTPS for transmission. 
                        // Set to `false` during development but should always be `true` in production to ensure cookies are sent only over secure HTTPS connections.  
                        Secure = false, // set to true in production to avoid Man-in-the-middle attacks
                        // Allows the cookie to be sent with cross-site requests, which is necessary in some scenarios (e.g., if the app interacts with APIs or resources hosted on different domains). 
                        // This setting is typically used for development but increases the risk of CSRF attacks in production unless proper safeguards (like anti-CSRF tokens) are implemented.
                        SameSite = SameSiteMode.None,   // set to Strict in production to avoid CSRF attacks
                        Path = "/api/auth",
                        Expires = DateTime.UtcNow.AddDays(-1), // Expire immediately
                    }
                );
                
                _apiResponse.IsSuccess = true;
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            else
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages = new List<string>
                {
                    "Invalid Input"
                };
                _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_apiResponse);
            }
        }
    }
}