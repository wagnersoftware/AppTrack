using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class AiSettings : BaseEntity
{
    public string ApiKey { get; set; } = string.Empty;

    public string PromptTemplate { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public ICollection<PromptParameter> PromptParameter { get; set; } = new List<PromptParameter>();
}
