using MagicVilla.Villa.Api.Models.Dtos;

namespace MagicVilla.Villa.Api.Data
{
    public static class VillaStore
    {
        public static readonly List<VillaDto> VillaDtos = new List<VillaDto>() {
                new() {
                    Id = 1,
                    Name = "Beach View",
                    Occupancy = 4,
                    Sqft = 100
                },
                new() {
                    Id = 2,
                    Name = "Pool View",
                    Occupancy = 4,
                    Sqft = 100
                }
        };
    }
}