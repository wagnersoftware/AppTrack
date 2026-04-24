using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.RssFeed;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using FluentValidation;

namespace AppTrack.Application.Features.RssFeeds.Commands.UpdateRssMonitoringSettings;

public sealed class UpdateRssMonitoringSettingsCommandHandler
    : IRequestHandler<UpdateRssMonitoringSettingsCommand, Unit>
{
    private readonly IRssMonitoringSettingsRepository _repository;
    private readonly IValidator<UpdateRssMonitoringSettingsCommand> _validator;

    public UpdateRssMonitoringSettingsCommandHandler(
        IRssMonitoringSettingsRepository repository,
        IValidator<UpdateRssMonitoringSettingsCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Unit> Handle(UpdateRssMonitoringSettingsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid update RSS monitoring settings request", validationResult);

        await _repository.UpsertAsync(new RssMonitoringSettings
        {
            UserId = request.UserId,
            NotificationEmail = request.NotificationEmail,
            Keywords = request.Keywords,
            PollIntervalMinutes = request.PollIntervalMinutes,
            NotifyByEmail = request.NotifyByEmail
        });

        return Unit.Value;
    }
}
