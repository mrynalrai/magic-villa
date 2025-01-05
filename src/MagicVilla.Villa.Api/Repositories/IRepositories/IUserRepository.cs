using MagicVilla.Villa.Api.Models;

namespace MagicVilla.Villa.Api.Repositories.IRepositories
{
    // public interface IUserRepository : IRepository<LocalUser>
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        bool IsUniqueUser(string username);
    }
}