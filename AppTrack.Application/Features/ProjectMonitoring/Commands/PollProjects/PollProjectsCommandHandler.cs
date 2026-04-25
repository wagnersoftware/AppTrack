using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Models;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.PollProjects;

public class PollProjectsCommandHandler : IRequestHandler<PollProjectsCommand, Unit>
{
    private readonly IUserPortalSubscriptionRepository _subscriptionRepository;
    private readonly IProjectMonitoringSettingsRepository _settingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IProcessedProjectItemRepository _processedRepository;
    private readonly IScrapedProjectRepository _scrapedProjectRepository;
    private readonly IProjectMatchNotifier _notifier;
    private readonly IUnitOfWork _unitOfWork;

    public PollProjectsCommandHandler(
        IUserPortalSubscriptionRepository subscriptionRepository,
        IProjectMonitoringSettingsRepository settingsRepository,
        IJobApplicationRepository jobApplicationRepository,
        IProcessedProjectItemRepository processedRepository,
        IScrapedProjectRepository scrapedProjectRepository,
        IProjectMatchNotifier notifier,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _settingsRepository = settingsRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _processedRepository = processedRepository;
        _scrapedProjectRepository = scrapedProjectRepository;
        _notifier = notifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(PollProjectsCommand request, CancellationToken cancellationToken)
    {
        var allActiveSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsWithPortalsAsync();
        var byUser = allActiveSubscriptions.GroupBy(s => s.UserId);

        foreach (var userGroup in byUser)
        {
            var userId = userGroup.Key;
            var settings = await _settingsRepository.GetByUserIdAsync(userId);

            if (settings is null || settings.Keywords.Count == 0)
                continue;

            var isPollDue = settings.LastPolledAt is null ||
                DateTime.UtcNow >= settings.LastPolledAt.Value.AddMinutes(settings.PollIntervalMinutes);
            if (!isPollDue)
                continue;

            var portalIds = userGroup.Select(s => s.ProjectPortalId).ToList();
            var scrapedProjects = await _scrapedProjectRepository.GetByPortalIdsAsync(portalIds);

            var urls = scrapedProjects.Select(p => p.Url).ToList();
            var processedUrls = await _processedRepository.GetProcessedUrlsAsync(userId, urls);

            var newProjects = scrapedProjects.Where(p => !processedUrls.Contains(p.Url)).ToList();
            var matches = newProjects.Where(p => MatchesKeywords(p, settings.Keywords)).ToList();

            if (newProjects.Count == 0)
                continue;

            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                foreach (var match in matches)
                {
                    await _jobApplicationRepository.CreateAsync(new JobApplication
                    {
                        UserId = userId,
                        Name = string.IsNullOrEmpty(match.CompanyName) ? match.Title : match.CompanyName,
                        Position = match.Title,
                        URL = match.Url,
                        JobDescription = string.Empty,
                        Location = string.Empty,
                        ContactPerson = string.Empty,
                        DurationInMonths = string.Empty,
                        StartDate = DateTime.UtcNow,
                        Status = JobApplicationStatus.Discovered
                    });
                }

                var processedItems = newProjects.Select(p => new ProcessedProjectItem
                {
                    UserId = userId,
                    ProjectItemUrl = p.Url,
                    ProcessedAt = DateTime.UtcNow
                });
                await _processedRepository.AddRangeAsync(processedItems, ct);
            }, cancellationToken);

            settings.LastPolledAt = DateTime.UtcNow;
            await _settingsRepository.UpdateAsync(settings);

            if (matches.Count > 0 && settings.NotifyByEmail && !string.IsNullOrEmpty(settings.NotificationEmail))
            {
                var now = DateTime.UtcNow;
                var isNotificationDue = settings.LastNotifiedAt is null ||
                    now >= settings.LastNotifiedAt.Value.AddMinutes(settings.NotificationIntervalMinutes);

                if (isNotificationDue)
                {
                    var notificationData = matches
                        .Select(m => new ScrapedProjectData(
                            m.Title,
                            m.Url,
                            string.Empty,
                            m.CompanyName,
                            m.ProjectPortal?.Name ?? string.Empty))
                        .ToList();

                    await _notifier.NotifyAsync(userId, settings.NotificationEmail, notificationData, cancellationToken);

                    settings.LastNotifiedAt = now;
                    await _settingsRepository.UpdateAsync(settings);
                }
            }
        }

        return Unit.Value;
    }

    private static bool MatchesKeywords(ScrapedProject project, List<string> keywords) =>
        keywords.Any(kw => project.Title.Contains(kw, StringComparison.OrdinalIgnoreCase));
}
