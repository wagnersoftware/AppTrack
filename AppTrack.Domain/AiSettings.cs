using AppTrack.Domain.Common;
using AppTrack.Domain.Enums;

namespace AppTrack.Domain;

public class AiSettings : BaseEntity
{
    public int SelectedChatModelId { get; set; }
    public AiResponseLanguage Language { get; set; } = AiResponseLanguage.English;

    public string UserId { get; set; } = string.Empty;

    public ICollection<PromptParameter> PromptParameter { get; set; } = new List<PromptParameter>();

    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
