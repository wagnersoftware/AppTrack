namespace AppTrack.Application.Features.RssFeeds.Dto;

public record RssMonitoringSettingsDto(List<string> Keywords, int PollIntervalMinutes, bool NotifyByEmail);
