using AppTrack.Application.Contracts.Mediator;
using AppTrack.Application.Features.ApplicationText.Dto;

namespace AppTrack.Application.Features.AiSettings.Queries.GetChatModelsQuery;

public class GetChatModelsQuery: IRequest<List<ChatModelDto>>
{
}
