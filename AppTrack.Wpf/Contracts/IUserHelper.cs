namespace AppTrack.WpfUi.Contracts
{
    public interface IUserHelper
    {
        Task<string?> TryGetUserIdAsync();
    }
}
