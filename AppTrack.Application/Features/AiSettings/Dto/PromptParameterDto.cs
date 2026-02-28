using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Application.Features.AiSettings.Dto
{
    public class PromptParameterDto : IPromptParameterValidatable
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public int Id { get; set; }
    }
}