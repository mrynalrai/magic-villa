using AutoMapper;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla.Villa.Api.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; 
        private readonly string _secretKey;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager,
            IUserRepository userRepository,
            IMapper mapper
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret") ?? throw new InvalidOperationException("The 'ApiSettings:Secret' configuration value is missing or null.");
            _userRepository = userRepository;
            _mapper = mapper;
        }


        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            // var user = _dbContext.LocalUsers.FirstOrDefault(user => 
            //     user.UserName.ToUpper() == loginRequestDto.UserName.ToUpper() &&
            //     user.Password == loginRequestDto.Password
            // );

            var user = await _userRepository.GetAsync(user => 
                user.UserName != null && 
                user.UserName.ToUpper() == loginRequestDto.UserName.ToUpper()); 

            bool isValid = user != null && await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (!isValid)
            {
                return new LoginResponseDto
                {
                    AccessToken = "",
                    User = null
                };
            }


            var roles = await _userManager.GetRolesAsync(user);

            var accessToken = await GetAccessToken(user, roles);
            var userResponseDto = _mapper.Map<UserDto>(user);
            userResponseDto.Role = roles.FirstOrDefault();
            LoginResponseDto loginResponseDto = new LoginResponseDto ()
            {
                AccessToken = accessToken,
                User = userResponseDto,
                // Role = roles.FirstOrDefault()
            };
            return loginResponseDto;
        }

        public Task<TokenDto> RefreshAccessToken(TokenDto tokenDto)
        {
            throw new NotImplementedException();
        }

        public async Task<UserDto> Register(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new() 
            {
                UserName = registrationRequestDto.UserName,
                // Password = registrationRequestDto.Password, 
                Email = registrationRequestDto.UserName,
                NormalizedEmail = registrationRequestDto.UserName.ToUpper(),
                Name = registrationRequestDto.Name
            };

            try 
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    // TODO - Allow admins to create roles
                    if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }
                    await _userManager.AddToRoleAsync(user, registrationRequestDto.Role);
                    var userToReturn = await _userRepository.GetAsync(u => u.UserName == registrationRequestDto.UserName);
                    
                    var roles = await _userManager.GetRolesAsync(user);
                    var userResponseDto = _mapper.Map<UserDto>(userToReturn);
                    userResponseDto.Role = roles.FirstOrDefault();

                    return userResponseDto;
                }
            }
            catch( Exception ex)
            {
                throw new Exception(ex.Message);
            }

            // await base.CreateAsync(user);
            // user.Password = "";
            // return user;

            return new UserDto();
        }
        
        private async Task<string> GetAccessToken(ApplicationUser user, IList<string>? roles)
        {
            // If user was found, generate JWT 
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    // new Claim(ClaimTypes.Role, user.Role)
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenStr = tokenHandler.WriteToken(token);
            return tokenStr;
        }
    }
}