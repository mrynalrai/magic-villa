using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;

namespace MagicVilla.Villa.Api.Repositories.IRepositories
{
    public interface IUserRepository : IRepository<LocalUser>
    {
        bool IsUniqueUser(string username);
        Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
        Task<LocalUser> Register(RegistrationRequestDto registrationRequestDto);
    }
}