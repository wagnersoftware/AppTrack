using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AutoMapper;

namespace AppTrack.Frontend.ApiService.Services;

public class ChatModelsService : BaseHttpService, IChatModelsService
{
    private readonly IMapper _mapper;

    public ChatModelsService(IClient client, ITokenStorage tokenStorage, IMapper mapper) : base(client, tokenStorage)
    {
        this._mapper = mapper;
    }

    public Task<Response<List<ChatModel>>> GetChatModels() =>
        TryExecuteAsync(async () =>
        {
            var chatModelDtos = await _client.ChatModelsAsync();
            return _mapper.Map<List<ChatModel>>(chatModelDtos);
        });
}
