using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Shared;
using Microsoft.Extensions.Logging;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.MatchProjects;

public class MatchProjectsCommandHandler : IRequestHandler<MatchProjectsCommand, Unit>
{
    private readonly IProjectMonitoringSettingsRepository _settingsRepository;
    private readonly IProcessedProjectItemRepository _processedRepository;
    private readonly IScrapedProjectRepository _scrapedRepository;
    private readonly IUserProjectMatchRepository _matchRepository;
    private readonly ILogger<MatchProjectsCommandHandler> _logger;

    public MatchProjectsCommandHandler(
        IProjectMonitoringSettingsRepository settingsRepository,
        IProcessedProjectItemRepository processedRepository,
        IScrapedProjectRepository scrapedRepository,
        IUserProjectMatchRepository matchRepository,
        ILogger<MatchProjectsCommandHandler> logger)
    {
        _settingsRepository = settingsRepository;
        _processedRepository = processedRepository;
        _scrapedRepository = scrapedRepository;
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(MatchProjectsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _settingsRepository.GetByUserIdAsync(request.UserId);
        if (settings == null)
        {
            _logger.LogInformation("No monitoring settings found for user {UserId}", request.UserId);
            return Unit.Value;
        }

        if (settings.Keywords.Count == 0)
        {
            _logger.LogInformation("No keywords configured for user {UserId}", request.UserId);
            return Unit.Value;
        }

        var processedUrls = await _processedRepository.GetProcessedUrlsAsync(request.UserId, new List<string>());
        var unprocessedProjects = await _scrapedRepository.GetUnprocessedForUserAsync(request.UserId, processedUrls, cancellationToken);

        var matches = unprocessedProjects
            .Where(p => MatchesKeywords(p.Title, p.Description, settings.Keywords))
            .ToList();

        if (matches.Count == 0)
        {
            _logger.LogDebug("No matching projects found for user {UserId}", request.UserId);
            return Unit.Value;
        }

        var projectIds = matches.Select(project => project.Id).ToList();
        foreach (var projectId in projectIds)
        {
            var existingMatch = await _matchRepository.GetByUserAndProjectAsync(
                request.UserId, projectId, cancellationToken);

            if (existingMatch == null)
            {
                var newMatch = new AppTrack.Domain.UserProjectMatch
                {
                    UserId = request.UserId,
                    ScrapedProjectId = projectId,
                    IsNotified = false
                };
                await _matchRepository.CreateAsync(newMatch);
            }
        }

        _logger.LogInformation("Matched {Count} projects for user {UserId}", matches.Count, request.UserId);
        return Unit.Value;
    }

    private static bool MatchesKeywords(string title, string description, List<string> keywords)
    {
        var fullText = $"{title} {description}".ToLowerInvariant();
        return keywords.Any(keyword => fullText.Contains(keyword.ToLowerInvariant()));
    }
}
