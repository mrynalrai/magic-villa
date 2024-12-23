using Microsoft.EntityFrameworkCore;
using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<VillaModel> Villas { get; set; }
    }
}