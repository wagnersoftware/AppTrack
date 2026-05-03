using AppTrack.Application.Contracts.Email;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Features.ProjectMonitoring.Commands.SendProjectNotifications;
using AppTrack.Application.Models.Email;
using AppTrack.Domain;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class SendProjectNotificationsCommandHandlerTests
{
    private readonly Mock<IUserProjectMatchRepository> _matchRepo = new();
    private readonly Mock<IProjectMonitoringSettingsRepository> _settingsRepo = new();
    private readonly Mock<IEmailSender> _emailSender = new();

    private SendProjectNotificationsCommandHandler CreateHandler() => new(
        _matchRepo.Object,
        _settingsRepo.Object,
        _emailSender.Object);

    [Fact]
    public async Task Handle_ShouldSendEmail_WhenUnnotifiedMatchesExistAndIntervalReached()
    {
        var portal = new ProjectPortal { Name = "Freelancermap" };
        var project = new ScrapedProject { Title = ".NET Dev", Url = "https://x.de/1", ProjectPortal = portal };
        var match = new UserProjectMatch { Id = 1, UserId = "u1", ScrapedProject = project, IsNotified = false };

        _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserProjectMatch> { match });
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
            .ReturnsAsync(new ProjectMonitoringSettings
            {
                UserId = "u1",
                NotificationEmail = "test@example.com",
                NotificationIntervalMinutes = 60,
                LastNotifiedAt = null
            });
        _emailSender.Setup(e => e.SendEmail(It.IsAny<EmailMessage>())).ReturnsAsync(true);
        _matchRepo.Setup(r => r.MarkNotifiedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _settingsRepo.Setup(r => r.UpdateAsync(It.IsAny<ProjectMonitoringSettings>())).Returns(Task.CompletedTask);

        await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

        _emailSender.Verify(e => e.SendEmail(It.Is<EmailMessage>(m => m.To == "test@example.com")), Times.Once);
        _matchRepo.Verify(r => r.MarkNotifiedAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(1)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotSendEmail_WhenNotificationIntervalNotReached()
    {
        var portal = new ProjectPortal { Name = "Freelancermap" };
        var project = new ScrapedProject { Title = ".NET Dev", Url = "https://x.de/1", ProjectPortal = portal };
        var match = new UserProjectMatch { Id = 2, UserId = "u1", ScrapedProject = project, IsNotified = false };

        _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserProjectMatch> { match });
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
            .ReturnsAsync(new ProjectMonitoringSettings
            {
                UserId = "u1",
                NotificationEmail = "test@example.com",
                NotificationIntervalMinutes = 60,
                LastNotifiedAt = DateTime.UtcNow.AddMinutes(-10)
            });

        await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

        _emailSender.Verify(e => e.SendEmail(It.IsAny<EmailMessage>()), Times.Never);
        _matchRepo.Verify(r => r.MarkNotifiedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDoNothing_WhenNoUnnotifiedMatches()
    {
        _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserProjectMatch>());

        await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

        _emailSender.Verify(e => e.SendEmail(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldUpdateLastNotifiedAt_AfterSendingEmail()
    {
        var portal = new ProjectPortal { Name = "Portal" };
        var project = new ScrapedProject { Title = "Dev", Url = "https://x.de/1", ProjectPortal = portal };
        var match = new UserProjectMatch { Id = 3, UserId = "u1", ScrapedProject = project, IsNotified = false };

        _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserProjectMatch> { match });

        ProjectMonitoringSettings? capturedSettings = null;
        var settings = new ProjectMonitoringSettings
        {
            UserId = "u1",
            NotificationEmail = "a@b.com",
            NotificationIntervalMinutes = 60,
            LastNotifiedAt = null
        };
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1")).ReturnsAsync(settings);
        _settingsRepo
            .Setup(r => r.UpdateAsync(It.IsAny<ProjectMonitoringSettings>()))
            .Callback<ProjectMonitoringSettings>(s => capturedSettings = s)
            .Returns(Task.CompletedTask);
        _emailSender.Setup(e => e.SendEmail(It.IsAny<EmailMessage>())).ReturnsAsync(true);
        _matchRepo.Setup(r => r.MarkNotifiedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

        capturedSettings.ShouldNotBeNull();
        capturedSettings!.LastNotifiedAt.ShouldNotBeNull();
        capturedSettings.LastNotifiedAt!.Value.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(-5));
    }

    [Fact]
    public async Task Handle_ShouldNotMarkNotified_WhenEmailSendFails()
    {
        var portal = new ProjectPortal { Name = "Portal" };
        var project = new ScrapedProject { Title = "Dev", Url = "https://x.de/1", ProjectPortal = portal };
        var match = new UserProjectMatch { Id = 4, UserId = "u1", ScrapedProject = project, IsNotified = false };

        _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<UserProjectMatch> { match });
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
            .ReturnsAsync(new ProjectMonitoringSettings
            {
                UserId = "u1",
                NotificationEmail = "a@b.com",
                NotificationIntervalMinutes = 60,
                LastNotifiedAt = null
            });
        _emailSender.Setup(e => e.SendEmail(It.IsAny<EmailMessage>())).ReturnsAsync(false);

        await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

        _matchRepo.Verify(r => r.MarkNotifiedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), Times.Never);
        _settingsRepo.Verify(r => r.UpdateAsync(It.IsAny<ProjectMonitoringSettings>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldProcessEachUserIndependently_WhenMultipleUsersHaveUnnotifiedMatches()
    {
        // Arrange — two users, both with unnotified matches and notification intervals that have elapsed
        var portal = new ProjectPortal { Name = "Freelancermap" };

        var projectU1 = new ScrapedProject { Title = ".NET Dev", Url = "https://x.de/u1", ProjectPortal = portal };
        var matchU1 = new UserProjectMatch { Id = 10, UserId = "u1", ScrapedProject = projectU1, IsNotified = false };

        var projectU2 = new ScrapedProject { Title = "C# Engineer", Url = "https://x.de/u2", ProjectPortal = portal };
        var matchU2 = new UserProjectMatch { Id = 11, UserId = "u2", ScrapedProject = projectU2, IsNotified = false };

        _matchRepo.Setup(r => r.GetUnnotifiedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserProjectMatch> { matchU1, matchU2 });

        _settingsRepo.Setup(r => r.GetByUserIdAsync("u1"))
            .ReturnsAsync(new ProjectMonitoringSettings
            {
                UserId = "u1",
                NotificationEmail = "u1@example.com",
                NotificationIntervalMinutes = 60,
                LastNotifiedAt = null
            });
        _settingsRepo.Setup(r => r.GetByUserIdAsync("u2"))
            .ReturnsAsync(new ProjectMonitoringSettings
            {
                UserId = "u2",
                NotificationEmail = "u2@example.com",
                NotificationIntervalMinutes = 60,
                LastNotifiedAt = null
            });

        _emailSender.Setup(e => e.SendEmail(It.IsAny<EmailMessage>())).ReturnsAsync(true);
        _matchRepo.Setup(r => r.MarkNotifiedAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _settingsRepo.Setup(r => r.UpdateAsync(It.IsAny<ProjectMonitoringSettings>())).Returns(Task.CompletedTask);

        // Act
        await CreateHandler().Handle(new SendProjectNotificationsCommand(), CancellationToken.None);

        // Assert — email sent to each user at their configured address
        _emailSender.Verify(e => e.SendEmail(It.Is<EmailMessage>(m => m.To == "u1@example.com")), Times.Once);
        _emailSender.Verify(e => e.SendEmail(It.Is<EmailMessage>(m => m.To == "u2@example.com")), Times.Once);
        _emailSender.Verify(e => e.SendEmail(It.IsAny<EmailMessage>()), Times.Exactly(2));

        // Assert — IsNotified set for both users' matches
        _matchRepo.Verify(r => r.MarkNotifiedAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(10)), It.IsAny<CancellationToken>()), Times.Once);
        _matchRepo.Verify(r => r.MarkNotifiedAsync(It.Is<IEnumerable<int>>(ids => ids.Contains(11)), It.IsAny<CancellationToken>()), Times.Once);
    }
}
