using AppTrack.Frontend.Models;

namespace AppTrack.WpfUi.Cache;

public interface IChatModelStore
{
    IReadOnlyList<ChatModel> ChatModels { get; }

    Task GetChatModelsAsync();
}
