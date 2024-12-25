using System.Linq.Expressions;
using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Repositories.IRepositories
{
    public interface IVillaRepository
    {
        Task<List<VillaModel>> GetAllAsync(Expression<Func<VillaModel, bool>> filter = null);
        Task<VillaModel> GetAsync(Expression<Func<VillaModel, bool>> filter = null, bool tracked=true);
        Task CreateAsync(VillaModel entity);
        Task UpdateAsync(VillaModel entity);
        Task RemoveAsync(VillaModel entity);
        Task SaveAsync();
    }
}