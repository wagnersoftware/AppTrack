using AppTrack.Application.Features.AiSettings.Commands.UpdateAiSettings;
using AppTrack.Application.Features.AiSettings.Dto;
using AppTrack.Application.Features.AiSettings.Queries.GetAiSettingsByUserId;
using AppTrack.Domain;

namespace AppTrack.Application.Mappings;

internal static class AiSettingsMappings
{
    internal static AiSettings ToDomain(this GetAiSettingsByUserIdQuery query) => new()
    {
        UserId = query.UserId,
    };

    internal static void ApplyTo(this UpdateAiSettingsCommand command, AiSettings entity)
    {
        entity.SelectedChatModelId = command.SelectedChatModelId;
        entity.UserId = command.UserId;

        entity.PromptParameter.Clear();
        foreach (var dto in command.PromptParameter)
        {
            entity.PromptParameter.Add(PromptParameter.Create(dto.Key, dto.Value));
        }

        entity.Prompts.Clear();
        foreach (var dto in command.Prompts)
        {
            entity.Prompts.Add(Prompt.Create(dto.Name, dto.PromptTemplate));
        }
    }

    internal static AiSettingsDto ToDto(this AiSettings entity) => new()
    {
        Id = entity.Id,
        SelectedChatModelId = entity.SelectedChatModelId,
        UserId = entity.UserId,
        PromptParameter = entity.PromptParameter.Select(p => p.ToDto()).ToList(),
        Prompts = entity.Prompts.Select(p => p.ToDto()).ToList(),
    };

    internal static PromptParameterDto ToDto(this PromptParameter entity) => new()
    {
        Id = entity.Id,
        Key = entity.Key,
        Value = entity.Value,
    };

    internal static PromptDto ToDto(this Prompt entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        PromptTemplate = entity.PromptTemplate,
    };
}
