using AppTrack.Domain;
using AppTrack.Persistance.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace AppTrack.Persistance.IntegrationTests
{
    public class AppTrackDatabaseContextTests
    {
        private readonly AppTrackDatabaseContext _appTrackDatabaseContext;

        public AppTrackDatabaseContextTests()
        {
          var dbOptions = new DbContextOptionsBuilder<AppTrackDatabaseContext>()
                .UseInMemoryDatabase(new Guid().ToString())
                .Options;

            _appTrackDatabaseContext = new AppTrackDatabaseContext(dbOptions);
        }
        [Fact]
        public async Task Save_SetDateCreatedValue()
        {
            //Arrange
            var jobApplication = new JobApplication()
            {
                Id = 1,
                ClientName = "TestClient1",
            };

            //Act
            await _appTrackDatabaseContext.JobApplications.AddAsync(jobApplication);
            await _appTrackDatabaseContext.SaveChangesAsync();

            //Assert
            jobApplication.CreationDate.ShouldNotBeNull();
        }

        [Fact]
        public async Task Save_SetDateModifiedValue()
        {
            //Arrange
            var jobApplication = new JobApplication()
            {
                Id = 1,
                ClientName = "TestClient1",
            };

            //Act
            await _appTrackDatabaseContext.JobApplications.AddAsync(jobApplication);
            await _appTrackDatabaseContext.SaveChangesAsync();

            //Assert
            jobApplication.ModifiedDate.ShouldNotBeNull();
        }
    }
}