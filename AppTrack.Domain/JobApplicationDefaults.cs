using AppTrack.Domain.Common;

namespace AppTrack.Domain;

public class JobApplicationDefaults : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public int UserId { get; set; } = default;
}
