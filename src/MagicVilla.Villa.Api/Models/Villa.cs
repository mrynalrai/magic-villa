namespace MagicVilla.Villa.Api.Models
{
    public class Villa
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime Created { get; set; }
    }
}