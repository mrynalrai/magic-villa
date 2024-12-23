using Microsoft.EntityFrameworkCore;
using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<VillaModel> Villas { get; set; }
    }
}