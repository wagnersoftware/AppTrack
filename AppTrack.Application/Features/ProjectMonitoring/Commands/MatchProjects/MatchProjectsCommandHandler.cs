using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;

public class MatchProjectsCommandHandler : IRequestHandler<MatchProjectsCommand, Unit>
{
    private readonly IUserPortalSubscriptionRepository _subscriptionRepository;
    private readonly IProjectMonitoringSettingsRepository _settingsRepository;
    private readonly IScrapedProjectRepository _scrapedProjectRepository;
    private readonly IUserProjectMatchRepository _matchRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IProcessedProjectItemRepository _processedRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MatchProjectsCommandHandler(
        IUserPortalSubscriptionRepository subscriptionRepository,
        IProjectMonitoringSettingsRepository settingsRepository,
        IScrapedProjectRepository scrapedProjectRepository,
        IUserProjectMatchRepository matchRepository,
        IJobApplicationRepository jobApplicationRepository,
        IProcessedProjectItemRepository processedRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _settingsRepository = settingsRepository;
        _scrapedProjectRepository = scrapedProjectRepository;
        _matchRepository = matchRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _processedRepository = processedRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(MatchProjectsCommand request, CancellationToken cancellationToken)
    {
        var allSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsWithPortalsAsync();
        var byUser = allSubscriptions.GroupBy(s => s.UserId);

        foreach (var userGroup in byUser)
        {
            var userId = userGroup.Key;
            var settings = await _settingsRepository.GetByUserIdAsync(userId);

            if (settings is null || settings.Keywords.Count == 0)
                continue;

            var portalIds = userGroup.Select(s => s.ProjectPortalId).ToList();
            var newProjects = await _scrapedProjectRepository.GetUnprocessedForUserAsync(userId, portalIds, cancellationToken);

            if (newProjects.Count == 0)
                continue;

            var matches = newProjects
                .Where(p => settings.Keywords.Any(kw =>
                    p.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (matches.Count > 0)
                {
                    var userMatches = matches.Select(m => new UserProjectMatch
                    {
                        UserId = userId,
                        ScrapedProjectId = m.Id,
                        IsNotified = false
                    }).ToList();
                    await _matchRepository.AddRangeAsync(userMatches, ct);

                    foreach (var match in matches)
                    {
                        await _jobApplicationRepository.CreateAsync(new JobApplication
                        {
                            UserId = userId,
                            Name = string.IsNullOrEmpty(match.CompanyName) ? match.Title : match.CompanyName,
                            Position = match.Title,
                            URL = match.Url,
                            JobDescription = match.Description ?? string.Empty,
                            Location = string.Empty,
                            ContactPerson = string.Empty,
                            DurationInMonths = string.Empty,
                            StartDate = DateTime.UtcNow,
                            Status = JobApplicationStatus.Discovered
                        });
                    }
                }

                var processedItems = newProjects.Select(p => new ProcessedProjectItem
                {
                    UserId = userId,
                    ProjectItemUrl = p.Url,
                    ProcessedAt = DateTime.UtcNow
                });
                await _processedRepository.AddRangeAsync(processedItems, ct);
            }, cancellationToken);
        }

        return Unit.Value;
    }
}
