using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using FluentValidation;

namespace AppTrack.Application.Features.RssFeeds.Commands.SetRssSubscriptions;

public class SetRssSubscriptionsCommandHandler : IRequestHandler<SetRssSubscriptionsCommand, Unit>
{
    private readonly IUserRssSubscriptionRepository _repository;
    private readonly IValidator<SetRssSubscriptionsCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public SetRssSubscriptionsCommandHandler(
        IUserRssSubscriptionRepository repository,
        IValidator<SetRssSubscriptionsCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetRssSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid request", validationResult);

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var subscription in request.Subscriptions)
                await _repository.UpsertAsync(request.UserId, subscription.PortalId, subscription.IsActive);
        }, cancellationToken);

        return Unit.Value;
    }
}
