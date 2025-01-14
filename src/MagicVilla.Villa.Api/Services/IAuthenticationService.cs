using MagicVilla.Villa.Api.Models.Dtos;

namespace MagicVilla.Villa.Api.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<UserDto> Register(RegistrationRequestDto registrationRequestDto);
        // Task<LocalUser> Register(RegistrationRequestDto registrationRequestDto);
        Task<TokenDto> RefreshAccessToken(TokenDto tokenDto);
        Task RevokeRefreshToken(TokenDto tokenDto);
    }
}