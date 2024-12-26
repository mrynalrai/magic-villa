using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Repositories.IRepositories;

namespace MagicVilla.Villa.Api.Repositories
{
    public class VillaNumberRepository : Repository<Models.Villa>, IVillaNumberRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VillaNumberRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<VillaNumber> UpdateAsync(VillaNumber entity)
        {
            _dbContext.VillaNumbers.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}