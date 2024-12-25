using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Repositories.IRepositories;

namespace MagicVilla.Villa.Api.Repositories
{
    public class VillaRepository : Repository<Models.Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public VillaRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Models.Villa> UpdateAsync(Models.Villa entity)
        {
            _dbContext.Villas.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}