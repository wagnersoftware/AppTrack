using AppTrack.Application.Features.FreelancerProfile.Queries.GetFreelancerProfile;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.FreelancerProfile.Queries;

public class GetFreelancerProfileQueryValidatorTests
{
    private readonly GetFreelancerProfileQueryValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldPass_ForAnyQuery()
    {
        // Arrange
        var query = new GetFreelancerProfileQuery { UserId = "user-1" };

        // Act
        var result = await _validator.TestValidateAsync(query);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
