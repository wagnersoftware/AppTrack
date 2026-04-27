using AppTrack.Domain;

namespace AppTrack.Application.Contracts.ProjectMonitoring;

public interface IUserProjectMatchRepository
{
    Task AddRangeAsync(IEnumerable<UserProjectMatch> matches, CancellationToken ct);

    /// <summary>
    /// Returns all IsNotified=false matches for users with NotifyByEmail=true
    /// and a non-empty NotificationEmail. Eager-loads ScrapedProject and ProjectPortal.
    /// </summary>
    Task<List<UserProjectMatch>> GetUnnotifiedAsync(CancellationToken ct);

    /// <summary>Sets IsNotified=true for the given match IDs.</summary>
    Task MarkNotifiedAsync(IEnumerable<int> matchIds, CancellationToken ct);
}
