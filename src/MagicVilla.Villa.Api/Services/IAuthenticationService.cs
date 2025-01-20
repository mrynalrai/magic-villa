using MagicVilla.Villa.Api.Models.Dtos;

namespace MagicVilla.Villa.Api.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<UserDto> Register(RegistrationRequestDto registrationRequestDto);
        // Task<LocalUser> Register(RegistrationRequestDto registrationRequestDto);
        Task<TokenDto> RefreshAccessToken(string refreshToken);   // When using OAuth 2.0 to generate a new access token using a refresh token, you should not pass the access token alongside the refresh token; only the refresh token is needed to request a new access token from the authorization server as the access token is considered expired and not necessary for the refresh process. 
        Task RevokeRefreshToken(string refreshToken);
    }
}