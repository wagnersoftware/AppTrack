using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class AiSettings : BaseEntity
{
    public int SelectedChatModelId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ICollection<PromptParameter> PromptParameter { get; set; } = new List<PromptParameter>();

    public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
}
