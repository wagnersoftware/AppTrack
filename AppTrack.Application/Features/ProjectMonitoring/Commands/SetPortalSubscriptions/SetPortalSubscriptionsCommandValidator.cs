using FluentValidation;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.SetPortalSubscriptions;

public class SetPortalSubscriptionsCommandValidator : AbstractValidator<SetPortalSubscriptionsCommand>
{
    public SetPortalSubscriptionsCommandValidator()
    {
        RuleFor(x => x.Subscriptions).NotNull();
        RuleForEach(x => x.Subscriptions).ChildRules(sub =>
            sub.RuleFor(s => s.PortalId).GreaterThan(0));
    }
}
