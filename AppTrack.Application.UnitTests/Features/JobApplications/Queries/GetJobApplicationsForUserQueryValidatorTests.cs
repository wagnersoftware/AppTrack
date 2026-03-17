using AppTrack.Application.Features.JobApplications.Queries.GetAllJobApplicationsForUser;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplications.Queries;

public class GetJobApplicationsForUserQueryValidatorTests
{
    private readonly GetJobApplicationsForUserQueryValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenQueryHasUserId()
    {
        var query = new GetJobApplicationsForUserQuery { UserId = "user-1" };
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenUserIdIsEmpty()
    {
        // The validator has no rules; all inputs are valid.
        var query = new GetJobApplicationsForUserQuery { UserId = string.Empty };
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }
}
