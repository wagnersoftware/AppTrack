using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.ApiService.Mappings;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Services;

public class ChatModelsService : BaseHttpService, IChatModelsService
{
    public ChatModelsService(IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
    {
    }

    public Task<Response<List<ChatModel>>> GetChatModels() =>
        TryExecuteAsync(async () =>
        {
            var chatModelDtos = await _client.ChatModelsAsync();
            return chatModelDtos.Select(dto => dto.ToModel()).ToList();
        });
}
