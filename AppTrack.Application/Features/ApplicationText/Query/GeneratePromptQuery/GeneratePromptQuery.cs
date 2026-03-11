using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ApplicationText.Dto;

namespace AppTrack.Application.Features.ApplicationText.Query.GeneratePromptQuery;

public class GeneratePromptQuery : IRequest<GeneratedPromptDto>, IUserScopedRequest
{
    public int JobApplicationId { get; set; }
    public string UserId { get; set; } = string.Empty;
}
