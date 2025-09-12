namespace AppTrack.Application.Contracts.Persistance;
public interface IGenericRepository<T> where T : class
{
    Task<T> GetAsync();

    Task<T> GetByIdAsync(int id);

    Task<T> UpdateAsync(T entity);

    Task<T> CreateAsync(T entity);

    Task<T> DeleteAsync(T entity);
}