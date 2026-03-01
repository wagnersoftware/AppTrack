using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Contracts.Persistance;
using AppTrack.Application.Features.ApplicationText.Dto;
using AutoMapper;

namespace AppTrack.Application.Features.AiSettings.Queries.GetChatModelsQuery;

public class GetChatModelsQueryHandler : IRequestHandler<GetChatModelsQuery, List<ChatModelDto>>
{
    private readonly IMapper _mapper;
    private readonly IChatModelRepository _chatModelRepository;

    public GetChatModelsQueryHandler(IMapper mapper, IChatModelRepository chatModelRepository)
    {
        this._mapper = mapper;
        this._chatModelRepository = chatModelRepository;
    }

    public async Task<List<ChatModelDto>> Handle(GetChatModelsQuery request, CancellationToken cancellationToken)
    {
        var chatModels = await _chatModelRepository.GetAsync();
        return _mapper.Map<List<ChatModelDto>>(chatModels);
    }
}
