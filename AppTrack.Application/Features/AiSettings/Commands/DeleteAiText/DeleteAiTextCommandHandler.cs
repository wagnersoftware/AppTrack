using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Exceptions;
using AppTrack.Application.Shared;
using FluentValidation;

namespace AppTrack.Application.Features.AiSettings.Commands.DeleteAiText;

public class DeleteAiTextCommandHandler : IRequestHandler<DeleteAiTextCommand, Unit>
{
    private readonly IJobApplicationAiTextRepository _aiTextRepository;
    private readonly IValidator<DeleteAiTextCommand> _validator;

    public DeleteAiTextCommandHandler(
        IJobApplicationAiTextRepository aiTextRepository,
        IValidator<DeleteAiTextCommand> validator)
    {
        _aiTextRepository = aiTextRepository;
        _validator = validator;
    }

    public async Task<Unit> Handle(DeleteAiTextCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (validationResult.Errors.Count > 0)
        {
            throw new BadRequestException("Invalid delete AI text request.", validationResult);
        }

        var aiText = await _aiTextRepository.GetByIdAsync(request.Id);
        await _aiTextRepository.DeleteAsync(aiText!);

        return Unit.Value;
    }
}
