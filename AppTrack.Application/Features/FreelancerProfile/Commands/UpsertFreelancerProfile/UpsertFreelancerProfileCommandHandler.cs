using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Features.FreelancerProfile.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.FreelancerProfile.Commands.UpsertFreelancerProfile;

public class UpsertFreelancerProfileCommandHandler : IRequestHandler<UpsertFreelancerProfileCommand, FreelancerProfileDto>
{
    private const string BuiltInPrefix = "builtIn_";

    private readonly IFreelancerProfileRepository _repository;
    private readonly IAiSettingsRepository _aiSettingsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertFreelancerProfileCommandHandler(
        IFreelancerProfileRepository repository,
        IAiSettingsRepository aiSettingsRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _aiSettingsRepository = aiSettingsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FreelancerProfileDto> Handle(UpsertFreelancerProfileCommand request, CancellationToken cancellationToken)
    {
        var validator = new UpsertFreelancerProfileCommandValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new BadRequestException("Invalid freelancer profile request", validationResult);
        }

        var existing = await _repository.GetByUserIdAsync(request.UserId);

        AppTrack.Domain.FreelancerProfile savedProfile = null!;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (existing == null)
            {
                var newProfile = request.ToNewDomain();
                await _repository.UpsertAsync(newProfile);
                savedProfile = newProfile;
            }
            else
            {
                request.ApplyTo(existing);
                await _repository.UpsertAsync(existing);
                savedProfile = existing;
            }

            await SyncBuiltInParametersAsync(savedProfile);
        }, cancellationToken);

        return savedProfile.ToDto();
    }

    private async Task SyncBuiltInParametersAsync(AppTrack.Domain.FreelancerProfile profile)
    {
        var aiSettings = await _aiSettingsRepository.GetByUserIdWithPromptParameterAsync(profile.UserId);

        if (aiSettings is null)
        {
            aiSettings = new AppTrack.Domain.AiSettings { UserId = profile.UserId };
            await _aiSettingsRepository.CreateAsync(aiSettings);
        }

        var parameters = new Dictionary<string, string?>
        {
            [$"{BuiltInPrefix}FirstName"] = profile.FirstName,
            [$"{BuiltInPrefix}LastName"] = profile.LastName,
            [$"{BuiltInPrefix}HourlyRate"] = profile.HourlyRate?.ToString(),
            [$"{BuiltInPrefix}DailyRate"] = profile.DailyRate?.ToString(),
            [$"{BuiltInPrefix}AvailableFrom"] = profile.AvailableFrom?.ToString("yyyy-MM-dd"),
            [$"{BuiltInPrefix}WorkMode"] = profile.WorkMode?.ToString(),
            [$"{BuiltInPrefix}Skills"] = profile.Skills,
            [$"{BuiltInPrefix}CvText"] = profile.CvText,
        };

        foreach (var (key, value) in parameters)
        {
            var existingParam = aiSettings.BuiltInPromptParameter
                .FirstOrDefault(p => p.Key == key);

            if (string.IsNullOrEmpty(value))
            {
                if (existingParam is not null)
                {
                    aiSettings.BuiltInPromptParameter.Remove(existingParam);
                }
            }
            else
            {
                if (existingParam is not null)
                {
                    existingParam.Value = value;
                }
                else
                {
                    aiSettings.BuiltInPromptParameter.Add(AppTrack.Domain.BuiltInPromptParameter.Create(key, value));
                }
            }
        }

        await _aiSettingsRepository.UpdateAsync(aiSettings);
    }
}
