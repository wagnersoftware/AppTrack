using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.ApiService.Contracts;

namespace AppTrack.Frontend.ApiService.Services
{
    public class ApplicationTextService : BaseHttpService, IApplicationTextService
    {
        public ApplicationTextService(IClient client, ITokenStorage tokenStorage) : base(client, tokenStorage)
        {
        }

        public async Task<Response<string>> GenerateApplicationText(int applicationId, int userId, string url, string position)
        {
            try
            {
                var command = new GenerateApplicationTextCommand()
                {
                    ApplicationId = applicationId,
                    UserId = userId,
                    Url = url,
                    Position = position
                };

                var generatedTextDto = await _client.GenerateApplicationTextAsync(command);
                return new Response<string> (){ Data = generatedTextDto.ApplicationText };
            }
            catch (ApiException e)
            {
                return ConvertApiException<string>(e);
            } 
        }
    }
}
