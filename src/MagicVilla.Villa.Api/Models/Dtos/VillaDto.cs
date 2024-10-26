using System.ComponentModel.DataAnnotations;

namespace MagicVilla.Villa.Api.Models.Dtos
{
    public class VillaDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public required string Name { get; set; }
    }
}