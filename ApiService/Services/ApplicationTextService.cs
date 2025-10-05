using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;

namespace AppTrack.Frontend.ApiService.Services
{
    public class ApplicationTextService : BaseHttpService, IApplicationTextService
    {
        public ApplicationTextService(IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
        {
        }

        public Task<Response<string>> GenerateApplicationText(int applicationId, string userId, string url, string position) =>
            TryExecuteAsync(async () =>
            {
                var command = new GenerateApplicationTextCommand()
                {
                    ApplicationId = applicationId,
                    UserId = userId,
                    Url = url,
                    Position = position
                };

                var generatedTextDto = await _client.GenerateApplicationTextAsync(command);
                return generatedTextDto.ApplicationText;
            });
    }
}
