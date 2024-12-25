using VillaModel = MagicVilla.Villa.Api.Models.Villa;

namespace MagicVilla.Villa.Api.Repositories.IRepositories
{
    public interface IVillaRepository : IRepository<VillaModel>
    {
        Task<VillaModel> UpdateAsync(VillaModel entity);        
    }
}