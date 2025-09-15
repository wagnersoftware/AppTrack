using AppTrack.Application.Contracts.Persistance;
using AppTrack.Domain;
using Moq;

namespace AppTrack.Application.UnitTests.Mocks;

public class MockJobApplicationRepository
{
    public static Mock<IJobApplicationRepository> GetJobApplicationRepository()
    {
        var jobApplications = new List<JobApplication>()
        {
            new JobApplication()
            {
                Id = 1,
                Client = "TestClient1",
            },
            new JobApplication()
            {
                Id = 2,
                Client = "TestClient2",
            }
        };

        var mockRepo = new Mock<IJobApplicationRepository>();

        mockRepo.Setup(r => r.GetAsync()).ReturnsAsync(jobApplications);

        mockRepo.Setup(r => r.CreateAsync(It.IsAny<JobApplication>())).Returns((JobApplication jobApplication) =>
        {
            jobApplications.Add(jobApplication);
            return Task.CompletedTask;

        });

        return mockRepo;
    }
}
