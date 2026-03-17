using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.JobApplications.Queries.GetJobApplicationById;
using AppTrack.Domain;
using FluentValidation.TestHelper;
using Moq;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Queries;

public class GetJobApplicationByIdQueryValidatorTests
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";
    private const int ExistingId = 7;

    private readonly Mock<IJobApplicationRepository> _mockRepo;
    private readonly GetJobApplicationByIdQueryValidator _validator;

    public GetJobApplicationByIdQueryValidatorTests()
    {
        _mockRepo = new Mock<IJobApplicationRepository>();

        var existingEntity = new JobApplication
        {
            Id = ExistingId,
            UserId = OwnerId,
            Name = "Existing Application"
        };

        _mockRepo
            .Setup(r => r.GetByIdAsync(ExistingId))
            .ReturnsAsync(existingEntity);

        _mockRepo
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id != ExistingId)))
            .ReturnsAsync((JobApplication?)null);

        _validator = new GetJobApplicationByIdQueryValidator(_mockRepo.Object);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenQueryIsValid()
    {
        var query = new GetJobApplicationByIdQuery { Id = ExistingId, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(query);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenIdIsZero()
    {
        var query = new GetJobApplicationByIdQuery { Id = 0, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(query);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id is required");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationDoesNotExist()
    {
        var query = new GetJobApplicationByIdQuery { Id = 9999, UserId = OwnerId };
        var result = await _validator.TestValidateAsync(query);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Id" && e.ErrorMessage == "Job application not found.");
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenJobApplicationBelongsToAnotherUser()
    {
        var query = new GetJobApplicationByIdQuery { Id = ExistingId, UserId = OtherId };
        var result = await _validator.TestValidateAsync(query);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "UserId" && e.ErrorMessage == "Job application doesn't belong to this user.");
    }
}
