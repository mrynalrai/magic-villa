namespace MagicVilla.Villa.Api.Models.Dtos
{
    public class LoginResponseDto
    {
        // public LocalUser User { get; set; }
        public UserDto User { get; set; }
        // public string Role { get; set; } You can extract role from Token
        public string AccessToken { get; set; }
    }
}