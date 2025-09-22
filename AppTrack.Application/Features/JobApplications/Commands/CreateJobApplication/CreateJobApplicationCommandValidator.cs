using AppTrack.Application.Contracts.Persistance;
using FluentValidation;

namespace AppTrack.Application.Features.JobApplications.Commands.CreateJobApplication;

public class CreateJobApplicationCommandValidator: AbstractValidator<CreateJobApplicationCommand>
{
    private readonly IJobApplicationRepository _jobApplicationRepository;

    public CreateJobApplicationCommandValidator(IJobApplicationRepository jobApplicationRepository)
    {
        _jobApplicationRepository = jobApplicationRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("{PropertyName} is required")
            .NotNull()
            .MaximumLength(50).WithMessage("{PropertyName} must be fewer than 50 characters");

        RuleFor(x => x)
            .MustAsync(ClientUnique)
            .WithMessage("Client name already exists");

    }

    private async Task<bool> ClientUnique(CreateJobApplicationCommand command, CancellationToken token)
    {
        return await _jobApplicationRepository.IsClientUnique(command.Name);
    }
}
