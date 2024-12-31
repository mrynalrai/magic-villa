using MagicVilla.Villa.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<VillaModel> Villas { get; set; }
        public DbSet<VillaNumber> VillaNumbers { get; set; }
        public DbSet<LocalUser> LocalUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var createdDate = new DateTime(2024, 12, 23, 0, 0, 0);
            modelBuilder.Entity<VillaModel>().HasData(
                new VillaModel()
                {
                    Id = 1,
                    Name = "Royal Villa",
                    Details = "Fusce 11 tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                    ImageUrl = "https://placehold.co/600x401",
                    Occupancy = 5,
                    Rate = 200,
                    Sqft = 550,
                    Amenity = "",
                    Created = createdDate
                },
                new VillaModel
                {
                    Id = 2,
                    Name = "Premium Pool Villa",
                    Details = "Fusce 11 tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                    ImageUrl = "https://placehold.co/600x402",
                    Occupancy = 4,
                    Rate = 300,
                    Sqft = 550,
                    Amenity = "",
                    Created = createdDate
                },
                new VillaModel
                {
                    Id = 3,
                    Name = "Luxury Pool Villa",
                    Details = "Fusce 11 tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                    ImageUrl = "https://placehold.co/600x403",
                    Occupancy = 4,
                    Rate = 400,
                    Sqft = 750,
                    Amenity = "",
                    Created = createdDate
                },
                new VillaModel
                {
                    Id = 4,
                    Name = "Diamond Villa",
                    Details = "Fusce 11 tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                    ImageUrl = "https://placehold.co/600x404",
                    Occupancy = 4,
                    Rate = 550,
                    Sqft = 900,
                    Amenity = "",
                    Created = createdDate
                },
                new VillaModel
                {
                    Id = 5,
                    Name = "Diamond Pool Villa",
                    Details = "Fusce 11 tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                    ImageUrl = "https://placehold.co/600x405",
                    Occupancy = 4,
                    Rate = 600,
                    Sqft = 1100,
                    Amenity = "",
                    Created = createdDate
                });
        }
    }
}