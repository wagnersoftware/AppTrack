using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using FluentValidation;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.SetPortalSubscriptions;

public class SetPortalSubscriptionsCommandHandler : IRequestHandler<SetPortalSubscriptionsCommand, Unit>
{
    private readonly IUserPortalSubscriptionRepository _repository;
    private readonly IValidator<SetPortalSubscriptionsCommand> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public SetPortalSubscriptionsCommandHandler(
        IUserPortalSubscriptionRepository repository,
        IValidator<SetPortalSubscriptionsCommand> validator,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _validator = validator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetPortalSubscriptionsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (validationResult.Errors.Count > 0)
            throw new BadRequestException("Invalid request", validationResult);

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var subscription in request.Subscriptions)
                await _repository.UpsertAsync(request.UserId, subscription.PortalId, subscription.IsActive);
        }, cancellationToken);

        return Unit.Value;
    }
}
