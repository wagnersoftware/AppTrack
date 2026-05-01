using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using System.Globalization;

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
                    ContainsKeywordAsTerm(p.Title, kw) ||
                    ContainsKeywordAsTerm(p.Description, kw)))
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
                            Location = string.IsNullOrEmpty(match.Location) ? "Unknown" : match.Location,
                            ContactPerson = string.IsNullOrEmpty(match.ContactPerson) ? "Unknown" : match.ContactPerson,
                            DurationInMonths = match.DurationInMonths,
                            StartDate = ParseStartDate(match.StartDateText),
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

    private static bool ContainsKeywordAsTerm(string text, string keyword)
    {
        var idx = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        while (idx >= 0)
        {
            var beforeOk = idx == 0 || !char.IsLetterOrDigit(text[idx - 1]);
            var afterOk = idx + keyword.Length >= text.Length || !char.IsLetterOrDigit(text[idx + keyword.Length]);
            if (beforeOk && afterOk) return true;
            idx = text.IndexOf(keyword, idx + 1, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private static DateTime ParseStartDate(string? text)
    {
        if (DateTime.TryParseExact(text, ["dd.MM.yyyy", "MM/yyyy", "MM.yyyy"],
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            return parsed;

        return DateTime.UtcNow.Date;
    }
}
