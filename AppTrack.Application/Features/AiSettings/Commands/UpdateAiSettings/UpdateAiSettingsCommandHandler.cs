using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Mappings;
using AppTrack.Application.Shared;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommandHandler : IRequestHandler<UpdateAiSettingsCommand, Unit>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public UpdateAiSettingsCommandHandler(IAiSettingsRepository aiSettingsRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
    }

    public async Task<Unit> Handle(UpdateAiSettingsCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateAiSettingsCommandValidator(_aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"One or more validation errors occurred.", validationResult);
        }

        var aiSettingsToUpdate = await _aiSettingsRepository.GetByIdWithPromptParameterAsync(request.Id);
        request.ApplyTo(aiSettingsToUpdate!);

        await _aiSettingsRepository.UpdateAsync(aiSettingsToUpdate!);

        return Unit.Value;
    }
}
