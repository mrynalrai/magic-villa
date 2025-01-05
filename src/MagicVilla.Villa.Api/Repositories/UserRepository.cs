using MagicVilla.Villa.Api.Data;
using MagicVilla.Villa.Api.Models;
using MagicVilla.Villa.Api.Repositories.IRepositories;

namespace MagicVilla.Villa.Api.Repositories
{
    // public class UserRepository : Repository<LocalUser>, IUserRepository
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public bool IsUniqueUser(string username)
        {
            var user = _dbContext.ApplicationUsers.FirstOrDefault(user => user.UserName != null && user.UserName.ToUpper() == username.ToUpper());
            return user == null;
        }
    }
}