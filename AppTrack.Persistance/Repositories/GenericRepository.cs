using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain.Common;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppTrackDatabaseContext _context;

    public GenericRepository(AppTrackDatabaseContext dbContext)
    {
        _context = dbContext;
    }

    public async Task CreateAsync(T entity)
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<T>> GetAsync()
    {
       return await _context.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
}
