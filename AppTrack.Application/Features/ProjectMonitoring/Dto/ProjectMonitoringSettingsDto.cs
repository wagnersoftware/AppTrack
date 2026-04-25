namespace AppTrack.Application.Features.ProjectMonitoring.Dto;

public record ProjectMonitoringSettingsDto(List<string> Keywords, int NotificationIntervalMinutes, bool NotifyByEmail, int PollIntervalMinutes);
