using AppTrack.Frontend.ApiService.Contracts;
using AppTrack.Frontend.Models;
using AppTrack.WpfUi.MessageBoxService;

namespace AppTrack.WpfUi.Cache;

public class ChatModelStore : IChatModelStore
{
    private readonly IChatModelsService _chatModelsService;
    private readonly IMessageBoxService _messageBoxService;

    public IReadOnlyList<ChatModel> ChatModels { get; private set; } = new List<ChatModel>();

    public ChatModelStore(IChatModelsService chatModelsService, IMessageBoxService messageBoxService)
    {
        this._chatModelsService = chatModelsService;
        this._messageBoxService = messageBoxService;
    }

    public async Task Initialize()
    {
        var apiResponse = await _chatModelsService.GetChatModels();

        if (apiResponse.Success == false)
        {
            _messageBoxService.ShowErrorMessageBox("Couldn't load chat models.");
        }

        ChatModels = apiResponse.Data!;
    }
}
