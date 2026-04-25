using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.ProjectMonitoring;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using AppTrack.Domain;
using FluentValidation;

namespace AppTrack.Application.Features.ProjectMonitoring.Commands.UpdateProjectMonitoringSettings;

public sealed class UpdateProjectMonitoringSettingsCommandHandler
    : IRequestHandler<UpdateProjectMonitoringSettingsCommand, Unit>
{
    private readonly IProjectMonitoringSettingsRepository _repository;
    private readonly IValidator<UpdateProjectMonitoringSettingsCommand> _validator;

    public UpdateProjectMonitoringSettingsCommandHandler(
        IProjectMonitoringSettingsRepository repository,
        IValidator<UpdateProjectMonitoringSettingsCommand> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<Unit> Handle(UpdateProjectMonitoringSettingsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
            throw new BadRequestException("Invalid update project monitoring settings request", validationResult);

        await _repository.UpsertAsync(new ProjectMonitoringSettings
        {
            UserId = request.UserId,
            NotificationEmail = request.NotificationEmail,
            Keywords = request.Keywords,
            NotificationIntervalMinutes = request.NotificationIntervalMinutes,
            PollIntervalMinutes = request.PollIntervalMinutes,
            NotifyByEmail = request.NotifyByEmail
        });

        return Unit.Value;
    }
}
