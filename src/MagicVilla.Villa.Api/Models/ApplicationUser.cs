using Microsoft.AspNetCore.Identity;

namespace MagicVilla.Villa.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}