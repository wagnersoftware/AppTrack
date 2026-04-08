namespace AppTrack.BlazorUi.Services;

/// <summary>
/// Tracks whether the profile-setup dialog has already been shown in this session,
/// so it is only displayed once after the first login — not on every navigation.
/// </summary>
public class ProfileSetupSessionState
{
    public bool HasChecked { get; set; }
}
