using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using AppTrack.Domain.Enums;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public static class MockFreelancerProfileRepository
{
    public const string ExistingUserId = "user-1";
    public const int ExistingId = 42;

    public static Mock<IFreelancerProfileRepository> GetMock()
    {
        var mockRepo = new Mock<IFreelancerProfileRepository>();

        var existingProfile = new FreelancerProfile
        {
            Id = ExistingId,
            UserId = ExistingUserId,
            FirstName = "Anna",
            LastName = "Müller",
            HourlyRate = 100m,
            DailyRate = null,
            WorkMode = RemotePreference.Remote,
            Skills = "C#, .NET",
        };

        mockRepo
            .Setup(r => r.GetByUserIdAsync(ExistingUserId))
            .ReturnsAsync(existingProfile);

        mockRepo
            .Setup(r => r.GetByUserIdAsync(It.Is<string>(id => id != ExistingUserId)))
            .ReturnsAsync((FreelancerProfile?)null);

        mockRepo
            .Setup(r => r.UpsertAsync(It.IsAny<FreelancerProfile>()))
            .Returns(Task.CompletedTask);

        return mockRepo;
    }
}
