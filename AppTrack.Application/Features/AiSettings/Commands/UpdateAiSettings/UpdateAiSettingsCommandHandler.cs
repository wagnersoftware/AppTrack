using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommandHandler : IRequestHandler<UpdateAiSettingsCommand, AiSettingsDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public UpdateAiSettingsCommandHandler(IAiSettingsRepository aiSettingsRepository)
    {
        _aiSettingsRepository = aiSettingsRepository;
    }

    public async Task<AiSettingsDto> Handle(UpdateAiSettingsCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpdateAiSettingsCommandValidator(_aiSettingsRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"One or more validation errors occurred.", validationResult);
        }

        var aiSettingsToUpdate = await _aiSettingsRepository.GetByIdIncludePromptParameterAsync(request.Id);
        request.ApplyTo(aiSettingsToUpdate!);

        await _aiSettingsRepository.UpdateAsync(aiSettingsToUpdate!);

        return aiSettingsToUpdate!.ToDto();
    }
}
