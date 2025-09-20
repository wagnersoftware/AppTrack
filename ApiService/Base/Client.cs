namespace AppTrack.Frontend.ApiService.Base;

public partial class Client : IClient
{
    public HttpClient HttpClient => _httpClient;
}
