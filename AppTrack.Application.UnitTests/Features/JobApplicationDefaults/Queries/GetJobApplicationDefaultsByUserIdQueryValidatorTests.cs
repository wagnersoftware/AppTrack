using AppTrack.Application.Features.JobApplicationDefaults.Queries.GetJobApplicationDefaultsByUserId;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.JobApplicationDefaults.Queries;

public class GetJobApplicationDefaultsByUserIdQueryValidatorTests
{
    private readonly GetJobApplicationDefaultsByUserIdQueryValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenQueryHasUserId()
    {
        var query = new GetJobApplicationDefaultsByUserIdQuery { UserId = "user-1" };
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenUserIdIsEmpty()
    {
        // The validator has no rules; all inputs are valid.
        var query = new GetJobApplicationDefaultsByUserIdQuery { UserId = string.Empty };
        var result = _validator.TestValidate(query);
        result.IsValid.ShouldBeTrue();
    }
}
