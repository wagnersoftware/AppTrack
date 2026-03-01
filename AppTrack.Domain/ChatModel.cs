using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class ChatModel: BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ApiModelName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
