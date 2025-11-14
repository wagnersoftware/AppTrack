using AppTrack.Frontend.Models.Base;

namespace AppTrack.Frontend.Models;

public class ChatModel : ModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ApiModelName { get; set; } = string.Empty;
}
