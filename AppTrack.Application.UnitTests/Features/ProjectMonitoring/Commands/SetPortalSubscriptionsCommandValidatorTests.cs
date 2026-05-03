using AppTrack.Application.Features.ProjectMonitoring.Commands.SetPortalSubscriptions;
using AppTrack.Application.Features.ProjectMonitoring.Dto;
using FluentValidation.TestHelper;
using Shouldly;

namespace AppTrack.Application.UnitTests.Features.ProjectMonitoring.Commands;

public class SetPortalSubscriptionsCommandValidatorTests
{
    private readonly SetPortalSubscriptionsCommandValidator _validator = new();

    private static SetPortalSubscriptionsCommand BuildValidCommand() => new()
    {
        UserId = "user-1",
        Subscriptions = new List<PortalSubscriptionItemDto>
        {
            new(PortalId: 1, IsActive: true)
        }
    };

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        // Arrange
        var command = BuildValidCommand();

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenSubscriptionsIsNull()
    {
        // Arrange
        var command = BuildValidCommand();
        command.Subscriptions = null!;

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Subscriptions);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenSubscriptionsIsEmpty()
    {
        // Arrange
        var command = BuildValidCommand();
        command.Subscriptions = new List<PortalSubscriptionItemDto>();

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenPortalIdIsZero()
    {
        // Arrange
        var command = BuildValidCommand();
        command.Subscriptions = new List<PortalSubscriptionItemDto>
        {
            new(PortalId: 0, IsActive: true)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_ShouldHaveError_WhenPortalIdIsNegative()
    {
        // Arrange
        var command = BuildValidCommand();
        command.Subscriptions = new List<PortalSubscriptionItemDto>
        {
            new(PortalId: -1, IsActive: true)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenPortalIdIsPositive()
    {
        // Arrange
        var command = BuildValidCommand();
        command.Subscriptions = new List<PortalSubscriptionItemDto>
        {
            new(PortalId: 42, IsActive: false)
        };

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
