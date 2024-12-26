using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MagicVilla.Villa.Api.Models
{
    public class VillaNumber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VillaNum { get; set; }   // to be provided by the user
        public string SpecialDetails { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        // Foreign key property
        public int VillaId { get; set; }
        // Navigation Property
        [ForeignKey("VillaId")]
        public Villa Villa { get; set; } // Reference to the parent Villa
    }
}