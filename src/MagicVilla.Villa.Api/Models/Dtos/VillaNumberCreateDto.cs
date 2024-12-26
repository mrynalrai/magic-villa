using System.ComponentModel.DataAnnotations;

namespace MagicVilla.Villa.Api.Models.Dtos
{
    public class VillaNumberCreateDto
    {
        [Required]
        public int VillaNum { get; set; }
        public string SpecialDetails { get; set; }
        [Required]
        public int VillaId { get; set; }
    }
}