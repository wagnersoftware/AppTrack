using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IChatModelsService
{
    Task<Response<List<ChatModel>>> GetChatModels();
}
