using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;
using System.Collections.ObjectModel;

namespace AppTrack.Frontend.ApiService.Mappings;

internal static class AiSettingsMappings
{
    internal static AiSettingsModel ToModel(this AiSettingsDto dto) => new()
    {
        Id = dto.Id,
        SelectedChatModelId = dto.SelectedChatModelId,
        ApiKey = dto.ApiKey ?? string.Empty,
        PromptTemplate = dto.PromptTemplate ?? string.Empty,
        UserId = dto.UserId ?? string.Empty,
        PromptParameter = new ObservableCollection<PromptParameterModel>(
            (dto.PromptParameter ?? []).Select(p => p.ToModel())),
    };

    internal static PromptParameterModel ToModel(this PromptParameterDto dto) => new()
    {
        Id = dto.Id,
        Key = dto.Key ?? string.Empty,
        Value = dto.Value ?? string.Empty,
    };

    internal static UpdateAiSettingsCommand ToUpdateCommand(this AiSettingsModel model) => new()
    {
        Id = model.Id,
        SelectedChatModelId = model.SelectedChatModelId,
        ApiKey = model.ApiKey,
        PromptTemplate = model.PromptTemplate,
        UserId = model.UserId,
        PromptParameter = model.PromptParameter.Select(p => p.ToDto()).ToList(),
    };

    internal static PromptParameterDto ToDto(this PromptParameterModel model) => new()
    {
        Id = model.Id,
        Key = model.Key,
        Value = model.Value,
    };
}
