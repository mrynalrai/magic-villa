using System.Linq.Expressions;

namespace MagicVilla.Villa.Api.Repositories.IRepositories
{
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string? includeProperties = null, int? pageSize = null, int? pageNumber = null);
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool tracked=true, string? includeProperties = null);
        Task CreateAsync(T entity);
        Task RemoveAsync(T entity);
        Task SaveAsync();
    }
}