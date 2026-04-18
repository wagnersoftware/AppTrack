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
        entity.Language = command.Language;
        entity.UserId = command.UserId;

        entity.PromptParameter.Clear();
        foreach (var dto in command.PromptParameter)
        {
            entity.PromptParameter.Add(PromptParameter.Create(dto.Key, dto.Value));
        }

        entity.Prompts.Clear();
        foreach (var dto in command.Prompts)
        {
            entity.Prompts.Add(Prompt.Create(dto.Key, dto.PromptTemplate));
        }
    }

    internal static AiSettingsDto ToDto(this AiSettings entity) => new()
    {
        Id = entity.Id,
        SelectedChatModelId = entity.SelectedChatModelId,
        Language = entity.Language,
        UserId = entity.UserId,
        PromptParameter = entity.PromptParameter.Select(p => p.ToDto()).ToList(),
        BuiltInPromptParameter = entity.BuiltInPromptParameter.Select(p => new PromptParameterDto { Id = p.Id, Key = p.Key, Value = p.Value }).ToList(),
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
        Key = entity.Name,
        PromptTemplate = entity.PromptTemplate,
    };

    internal static PromptDto ToDto(this BuiltInPrompt entity) => new()
    {
        Id = entity.Id,
        Key = entity.Name,
        PromptTemplate = entity.PromptTemplate,
    };
}
