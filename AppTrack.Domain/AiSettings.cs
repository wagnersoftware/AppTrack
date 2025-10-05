using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class AiSettings : BaseEntity
{
    public string ApiKey { get; set; } = string.Empty;

    public string MySkills { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
}
