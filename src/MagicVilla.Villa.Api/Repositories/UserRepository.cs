using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Models.Dtos;
using MagicVilla.Villa.Api.Repositories.IRepositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MagicVilla.Villa.Api.Repositories
{
    public class UserRepository : Repository<LocalUser>, IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager; 
        private readonly string _secretKey;
        private readonly IMapper _mapper;

        public UserRepository(
            ApplicationDbContext dbContext, 
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IMapper mapper,
            RoleManager<IdentityRole> roleManager
        ) : base(dbContext)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret") ?? throw new InvalidOperationException("The 'ApiSettings:Secret' configuration value is missing or null.");
            _mapper = mapper;
            _roleManager = roleManager;
        }
        public bool IsUniqueUser(string username)
        {
            var user = _dbContext.ApplicationUsers.FirstOrDefault(user => user.UserName != null && user.UserName.ToUpper() == username.ToUpper());
            if (user == null) 
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            // var user = _dbContext.LocalUsers.FirstOrDefault(user => 
            //     user.UserName.ToUpper() == loginRequestDto.UserName.ToUpper() &&
            //     user.Password == loginRequestDto.Password
            // );

            var user = _dbContext.ApplicationUsers.FirstOrDefault(user => 
                user.UserName != null && 
                user.UserName.ToUpper() == loginRequestDto.UserName.ToUpper()
            );

            bool isValid = user != null && await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);

            if (!isValid)
            {
                return new LoginResponseDto
                {
                    Token = "",
                    User = null
                };
            }

            // If user was found, generate JWT 
            var roles = await _userManager.GetRolesAsync(user);
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
            LoginResponseDto loginResponseDto = new LoginResponseDto ()
            {
                Token = tokenHandler.WriteToken(token),
                User = _mapper.Map<UserDto>(user),
                // Role = roles.FirstOrDefault()
            };
            return loginResponseDto;
        }

        public async Task<UserDto> Register(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new() 
            {
                UserName = registrationRequestDto.UserName,
                // Password = registrationRequestDto.Password, // TODO: Hash the password before saving it to the database
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
                    var userToReturn = _dbContext.ApplicationUsers.FirstOrDefault(u => u.UserName == registrationRequestDto.UserName);
                    return _mapper.Map<UserDto>(userToReturn);
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
    }
}