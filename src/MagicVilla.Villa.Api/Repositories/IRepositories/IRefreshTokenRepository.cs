using MagicVilla.Villa.Api.Models;

namespace MagicVilla.Villa.Api.Repositories.IRepositories
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken> UpdateAsync(RefreshToken entity);
        Task<IList<RefreshToken>> UpdateRangeAsync(IEnumerable<RefreshToken> entities);    
    }
}