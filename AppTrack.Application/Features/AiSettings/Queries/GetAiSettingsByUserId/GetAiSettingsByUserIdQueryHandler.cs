using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.AiSettings.Dto;
using AutoMapper;

namespace AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;

public class GetAiSettingsByUserIdQueryHandler : IRequestHandler<GetAiSettingsByUserIdQuery, AiSettingsDto>
{
    private readonly IMapper _mapper;
    private readonly IAiSettingsRepository _aiSettingsRepository;

    public GetAiSettingsByUserIdQueryHandler(IMapper mapper, IAiSettingsRepository aiSettingsRepository)
    {
        this._mapper = mapper;
        this._aiSettingsRepository = aiSettingsRepository;
    }

    /// <summary>
    /// Gets the AI settings for the specified user. Creates and returns a new instance, if the entity doesn't exist.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<AiSettingsDto> Handle(GetAiSettingsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var validator = new GetAiSettingsByUserIdQueryValidator();
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
        {
            throw new BadRequestException($"Invalid get AI settings request", validationResult);
        }

        var entity = await _aiSettingsRepository.GetByUserIdAsync(request.UserId);

        if (entity == null)
        {
            entity = _mapper.Map<Domain.AiSettings>(request);
            await _aiSettingsRepository.CreateAsync(entity);
        }

        var jobApplicationDto = _mapper.Map<AiSettingsDto>(entity);

        return jobApplicationDto;
    }
}
