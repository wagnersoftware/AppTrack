using AppTrack.Frontend.Models.Base;

namespace AppTrack.Frontend.Models;

public class JobApplicationDefaultsModel : ModelBase
{
    public string Name { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
}
