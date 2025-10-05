using AppTrack.Frontend.ApiService.ApiAuthenticationProvider;
using AppTrack.WpfUi.Contracts;
using AppTrack.WpfUi.MessageBoxService;
using System.Security.Claims;

namespace AppTrack.WpfUi.Helpers
{
    /// <summary>
    /// Provides functions for reading user data from authentication state.
    /// </summary>
    public class UserHelper: IUserHelper
    {
        private readonly ApiAuthenticationStateProvider _authProvider;
        private readonly IMessageBoxService _messageBoxService;

        public UserHelper(
            ApiAuthenticationStateProvider authProvider,
            IMessageBoxService messageBoxService)
        {
            _authProvider = authProvider;
            _messageBoxService = messageBoxService;
        }

        /// <summary>
        /// Tries to read the user id from the user claims and logs the user out, if not found.
        /// </summary>
        /// <returns></returns>
        public async Task<string?> TryGetUserIdAsync()
        {
            try
            {
                var authState = await _authProvider.GetAuthenticationStateAsync();
                var userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    await _authProvider.LoggedOut();
                    _messageBoxService.ShowErrorMessageBox("User ID not found. Please log in again.");

                    return null;
                }

                return userId;
            }
            catch (Exception ex)
            {
                _messageBoxService.ShowErrorMessageBox($"Error reading user ID: {ex.Message}");
                return null;
            }
        }
    }
}
