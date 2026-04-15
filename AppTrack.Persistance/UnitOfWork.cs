using AppTrack.Application.Contracts.Persistance;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace AppTrack.Persistance;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppTrackDatabaseContext _context;

    public UnitOfWork(AppTrackDatabaseContext context)
    {
        _context = context;
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await action(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}
