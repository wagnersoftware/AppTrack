using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public static class MockJobApplicationRepository
{
    public static Mock<IJobApplicationRepository> GetJobApplicationRepository()
    {
        var jobApplications = new List<JobApplication>()
        {
            new JobApplication()
            {
                Id = 1,
                Name = "TestClient1",
                UserId = "1"
            },
            new JobApplication()
            {
                Id = 2,
                Name = "TestClient2",
                UserId = "1"
            }
        };

        var mockRepo = new Mock<IJobApplicationRepository>();

        mockRepo.Setup(r => r.GetAllForUserAsync(It.IsAny<string>())).ReturnsAsync(jobApplications);

        mockRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns((JobApplication jobApplication) =>
        {
            jobApplications.Add(jobApplication);
            return Task.CompletedTask;

        });

        return mockRepo;
    }
}
