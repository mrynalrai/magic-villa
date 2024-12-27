namespace MagicVilla.Villa.Api.Models.Dtos
{
    public class LoginResponseDto
    {
        public LocalUser User { get; set; }
        public string Token { get; set; }
    }
}