using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using AutoMapper;

namespace AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;

public class UpdateAiSettingsCommandHandler : IRequestHandler<UpdateAiSettingsCommand, Unit>
{
    private readonly IMapper _mapper;
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public UpdateAiSettingsCommandHandler(IMapper mapper, IAiSettingsRepository aiSettingsRepository)
    {
        _mapper = mapper;
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
        _mapper.Map(request, aiSettingsToUpdate);

        await _aiSettingsRepository.UpdateAsync(aiSettingsToUpdate!);

        return Unit.Value;
    }
}
