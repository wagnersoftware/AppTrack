using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.ApplicationText.Dto;
using AppTrack.Application.Mappings;

namespace AppTrack.Application.Features.AiSettings.Queries.GetChatModelsQuery;

public class GetChatModelsQueryHandler : IRequestHandler<GetChatModelsQuery, List<ChatModelDto>>
{
    private readonly IChatModelRepository _chatModelRepository;

    public GetChatModelsQueryHandler(IChatModelRepository chatModelRepository)
    {
        this._chatModelRepository = chatModelRepository;
    }

    public async Task<List<ChatModelDto>> Handle(GetChatModelsQuery request, CancellationToken cancellationToken)
    {
        var chatModels = await _chatModelRepository.GetAsync();
        return chatModels.Where(m => m.IsActive).Select(m => m.ToDto()).ToList();
    }
}
