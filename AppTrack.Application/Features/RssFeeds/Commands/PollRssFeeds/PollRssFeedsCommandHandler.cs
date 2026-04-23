using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Features.RssFeeds.Models;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using AppTrack.Domain.Enums;

namespace AppTrack.Application.Features.RssFeeds.Commands.PollRssFeeds;

public class PollRssFeedsCommandHandler : IRequestHandler<PollRssFeedsCommand, Unit>
{
    private readonly IUserRssSubscriptionRepository _subscriptionRepository;
    private readonly IRssMonitoringSettingsRepository _settingsRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;
    private readonly IProcessedFeedItemRepository _processedRepository;
    private readonly IRssFeedReader _feedReader;
    private readonly IRssFeedItemParser _parser;
    private readonly IRssMatchNotifier _notifier;
    private readonly IUnitOfWork _unitOfWork;

    public PollRssFeedsCommandHandler(
        IUserRssSubscriptionRepository subscriptionRepository,
        IRssMonitoringSettingsRepository settingsRepository,
        IJobApplicationRepository jobApplicationRepository,
        IProcessedFeedItemRepository processedRepository,
        IRssFeedReader feedReader,
        IRssFeedItemParser parser,
        IRssMatchNotifier notifier,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _settingsRepository = settingsRepository;
        _jobApplicationRepository = jobApplicationRepository;
        _processedRepository = processedRepository;
        _feedReader = feedReader;
        _parser = parser;
        _notifier = notifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(PollRssFeedsCommand request, CancellationToken cancellationToken)
    {
        var allActiveSubscriptions = await _subscriptionRepository.GetActiveSubscriptionsWithPortalsAsync();
        var byUser = allActiveSubscriptions.GroupBy(s => s.UserId);

        foreach (var userGroup in byUser)
        {
            var userId = userGroup.Key;
            var settings = await _settingsRepository.GetByUserIdAsync(userId);

            if (settings is null || settings.Keywords.Count == 0)
                continue;

            if (string.IsNullOrEmpty(settings.NotificationEmail))
                continue;

            var now = DateTime.UtcNow;
            var dueSubscriptions = userGroup
                .Where(s => s.LastPolledAt is null ||
                            now >= s.LastPolledAt.Value.AddMinutes(settings.PollIntervalMinutes))
                .ToList();

            if (dueSubscriptions.Count == 0)
                continue;

            var allMatches = new List<RssJobApplicationData>();

            foreach (var subscription in dueSubscriptions)
            {
                var items = await _feedReader.ReadAsync(subscription.RssPortal.Url, cancellationToken);
                var urls = items.Select(i => i.Url).ToList();
                var processedUrls = await _processedRepository.GetProcessedUrlsAsync(userId, urls);

                var newItems = items.Where(i => !processedUrls.Contains(i.Url)).ToList();
                var matches = newItems
                    .Where(i => MatchesKeywords(i, settings.Keywords))
                    .Select(i => _parser.Parse(i, subscription.RssPortal.ParserType, subscription.RssPortal.Name))
                    .ToList();

                allMatches.AddRange(matches);

                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    foreach (var match in matches)
                    {
                        await _jobApplicationRepository.CreateAsync(new JobApplication
                        {
                            UserId = userId,
                            Name = string.IsNullOrEmpty(match.CompanyName) ? match.PortalName : match.CompanyName,
                            Position = match.Position,
                            URL = match.Url,
                            JobDescription = match.JobDescription,
                            Location = string.Empty,
                            ContactPerson = string.Empty,
                            DurationInMonths = string.Empty,
                            StartDate = DateTime.UtcNow,
                            Status = JobApplicationStatus.Discovered
                        });
                    }

                    var processedItems = newItems.Select(i => new ProcessedFeedItem
                    {
                        UserId = userId,
                        FeedItemUrl = i.Url,
                        ProcessedAt = DateTime.UtcNow
                    });
                    await _processedRepository.AddRangeAsync(processedItems, ct);

                    subscription.LastPolledAt = DateTime.UtcNow;
                    await _subscriptionRepository.UpdateAsync(subscription);
                }, cancellationToken);
            }

            if (allMatches.Count > 0)
                await _notifier.NotifyAsync(userId, settings.NotificationEmail, allMatches, cancellationToken);
        }

        return Unit.Value;
    }

    private static bool MatchesKeywords(RawFeedItem item, List<string> keywords) =>
        keywords.Any(kw =>
            item.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
            item.Description.Contains(kw, StringComparison.OrdinalIgnoreCase));
}
