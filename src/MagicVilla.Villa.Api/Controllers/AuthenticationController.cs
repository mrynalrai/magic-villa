using System.Net;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using MagicVilla.Villa.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla.Villa.Api.Controllers
{
    [Route("api/v{version:apiVersion}/auth")]
    [ApiController]
    [ApiVersionNeutral]
    public class AuthenticationController : ControllerBase
    {
        protected ApiResponse _apiResponse;
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(
            IUserRepository userRepository, 
            IAuthenticationService authenticationService
        )
        {
            _userRepository = userRepository;
            this._apiResponse = new();
            _authenticationService = authenticationService;
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
        public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDto tokenDto)
        {
            if (ModelState.IsValid)
            {
                var tokenDtoResponse = await _authenticationService.RefreshAccessToken(tokenDto);
                if (tokenDtoResponse == null || string.IsNullOrWhiteSpace(tokenDtoResponse.AccessToken))
                {
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages = new List<string>
                    {
                        "Something went wrong while generating access token"
                    };
                    _apiResponse.StatusCode = HttpStatusCode.InternalServerError;
                    return BadRequest(_apiResponse);
                }

                _apiResponse.StatusCode = HttpStatusCode.OK;
                _apiResponse.IsSuccess = true;
                _apiResponse.Result = tokenDto;
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