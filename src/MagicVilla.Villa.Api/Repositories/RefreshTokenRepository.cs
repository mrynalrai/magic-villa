using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Repositories.IRepositories;

namespace MagicVilla.Villa.Api.Repositories
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public RefreshTokenRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IList<RefreshToken>> UpdateRangeAsync(IEnumerable<RefreshToken> entities)
        {
            _dbContext.RefreshTokens.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
            return entities.ToList();
        }

        public async Task<RefreshToken> UpdateAsync(RefreshToken entity)
        {
            _dbContext.RefreshTokens.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
    }
}