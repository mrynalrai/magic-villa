using System.ComponentModel.DataAnnotations;

namespace MagicVilla.Villa.Api.Models.Dtos
{
    public class VillaUpdateDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        public required string Name { get; set; }
        public string Details { get; set; }
        [Required]
        public double Rate { get; set; }
        [Required]
        public int Occupancy { get; set; }
        [Required]
        public int Sqft { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; } 
        public IFormFile? Image { get; set; }
        public string Amenity { get; set; }
    }
}