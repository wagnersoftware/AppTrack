using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Mappings;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommandHandler : IRequestHandler<UpdateAiSettingsCommand, AiSettingsDto>
{
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IValidator<UpdateAiSettingsCommand> _validator;

    public UpdateAiSettingsCommandHandler(IAiSettingsRepository aiSettingsRepository, IValidator<UpdateAiSettingsCommand> validator)
    {
        _aiSettingsRepository = aiSettingsRepository;
        _validator = validator;
    }

    public async Task<AiSettingsDto> Handle(UpdateAiSettingsCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"One or more validation errors occurred.", validationResult);
        }

        var aiSettingsToUpdate = await _aiSettingsRepository.GetByIdWithPromptsAsync(request.Id);
        request.ApplyTo(aiSettingsToUpdate!);

        await _aiSettingsRepository.UpdateAsync(aiSettingsToUpdate!);

        return aiSettingsToUpdate!.ToDto();
    }
}
