using FluentValidation;

namespace AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;

public class SetRssSubscriptionsCommandValidator : AbstractValidator<SetRssSubscriptionsCommand>
{
    public SetRssSubscriptionsCommandValidator()
    {
        RuleFor(x => x.Subscriptions).NotNull();
        RuleForEach(x => x.Subscriptions).ChildRules(sub =>
            sub.RuleFor(s => s.PortalId).GreaterThan(0));
    }
}
