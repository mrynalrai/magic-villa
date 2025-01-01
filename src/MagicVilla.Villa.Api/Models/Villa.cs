using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla.Villa.Api.Models
{
    public class Villa
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Details { get; set; }
        public double Rate { get; set; } 
        public int Sqft { get; set; }
        public int Occupancy { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; } // It will have a reference to www. root folder where all images will be saved
        public string Amenity { get; set; }
        public DateTime Created { get; set; }
    }
}