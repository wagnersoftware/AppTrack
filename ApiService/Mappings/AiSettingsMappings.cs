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
        PromptParameter = new ObservableCollection<PromptParameterModel>(
            (dto.PromptParameter ?? []).Select(p => p.ToModel())),
        Prompts = new ObservableCollection<PromptModel>(
            (dto.Prompts ?? []).Select(p => p.ToModel())),
    };

    internal static PromptParameterModel ToModel(this PromptParameterDto dto) => new()
    {
        Id = dto.Id,
        Key = dto.Key ?? string.Empty,
        Value = dto.Value ?? string.Empty,
    };

    internal static PromptModel ToModel(this PromptDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name ?? string.Empty,
        PromptTemplate = dto.PromptTemplate ?? string.Empty,
    };

    internal static UpdateAiSettingsCommand ToUpdateCommand(this AiSettingsModel model) => new()
    {
        Id = model.Id,
        SelectedChatModelId = model.SelectedChatModelId,
        PromptParameter = model.PromptParameter.Select(p => p.ToDto()).ToList(),
        Prompts = model.Prompts.Select(p => p.ToDto()).ToList(),
    };

    internal static PromptParameterDto ToDto(this PromptParameterModel model) => new()
    {
        Id = model.Id,
        Key = model.Key,
        Value = model.Value,
    };

    internal static PromptDto ToDto(this PromptModel model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        PromptTemplate = model.PromptTemplate,
    };
}
