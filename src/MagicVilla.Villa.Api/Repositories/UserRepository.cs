using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using Microsoft.IdentityModel.Tokens;

namespace MagicVilla.Villa.Api.Repositories
{
    public class UserRepository : Repository<LocalUser>, IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _secretKey;

        public UserRepository(ApplicationDbContext dbContext, IConfiguration configuration) : base(dbContext)
        {
            _dbContext = dbContext;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret") ?? throw new InvalidOperationException("The 'ApiSettings:Secret' configuration value is missing or null.");
        }
        public bool IsUniqueUser(string username)
        {
            var user = _dbContext.LocalUsers.FirstOrDefault(user => user.UserName.ToUpper() == username.ToUpper());
            if (user == null) 
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = _dbContext.LocalUsers.FirstOrDefault(user => 
                user.UserName.ToUpper() == loginRequestDto.UserName.ToUpper() &&
                user.Password == loginRequestDto.Password
            );

            if (user == null)
            {
                return new LoginResponseDto()
                {
                    Token = "",
                    User = null
                };
            }

            // If user was found, generate JWT 

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDto loginResponseDto = new LoginResponseDto ()
            {
                Token = tokenHandler.WriteToken(token),
                User = user
            };
            return loginResponseDto;
        }

        public async Task<LocalUser> Register(RegistrationRequestDto registrationRequestDto)
        {
            LocalUser user = new() 
            {
                UserName = registrationRequestDto.UserName,
                Password = registrationRequestDto.Password, // TODO: Hash the password before saving it to the database
                Name = registrationRequestDto.Name,
                Role = registrationRequestDto.Role
            };

            await base.CreateAsync(user);
            user.Password = "";
            return user;
        }
    }
}