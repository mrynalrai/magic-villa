using MagicVilla.Villa.Api.Models;

namespace MagicVilla.Villa.Api.Repositories.IRepositories
{
    public interface IVillaNumberRepository: IRepository<VillaNumber>
    {
        Task<VillaNumber> UpdateAsync(VillaNumber entity);       
    }
}