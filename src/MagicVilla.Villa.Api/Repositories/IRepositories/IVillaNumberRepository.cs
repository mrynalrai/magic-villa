using MagicVilla.Villa.Api.Models;

namespace MagicVilla.Villa.Api.Repositories.IRepositories
{
    public interface IVillaNumberRepository
    {
        Task<VillaNumber> UpdateAsync(VillaNumber entity);       
    }
}