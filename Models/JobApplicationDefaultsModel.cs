using AppTrack.Frontend.Models.Base;
using AppTrack.Shared.Validation.Interfaces;

namespace AppTrack.Frontend.Models;

public class JobApplicationDefaultsModel : ModelBase, IJobApplicationDefaultsValidatable
{
    public string Name { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
}
