using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.DeleteAiText;

public class DeleteAiTextCommandValidator : AbstractValidator<DeleteAiTextCommand>
{
    private readonly IJobApplicationAiTextRepository _aiTextRepository;
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public DeleteAiTextCommandValidator(
        IJobApplicationAiTextRepository aiTextRepository,
        IJobApplicationRepository jobApplicationRepository)
    {
        _aiTextRepository = aiTextRepository;
        _jobApplicationRepository = jobApplicationRepository;

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0");

        RuleFor(x => x)
            .CustomAsync(ValidateOwnership)
            .When(x => x.Id > 0);
    }

    private async Task ValidateOwnership(
        DeleteAiTextCommand command,
        ValidationContext<DeleteAiTextCommand> context,
        CancellationToken token)
    {
        var aiText = await _aiTextRepository.GetByIdAsync(command.Id);

        if (aiText == null)
        {
            context.AddFailure("AI text entry not found");
            return;
        }

        var jobApplication = await _jobApplicationRepository.GetByIdAsync(aiText.JobApplicationId);

        if (jobApplication == null || jobApplication.UserId != command.UserId)
        {
            context.AddFailure("AI text entry does not belong to this user");
        }
    }
}
