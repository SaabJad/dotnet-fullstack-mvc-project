namespace MVCIDENTITYDEMO.Services
{
    public class MyService
    {
        private readonly HttpClient _client;

        public MyService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("SecureClient");
        }
    }
}
