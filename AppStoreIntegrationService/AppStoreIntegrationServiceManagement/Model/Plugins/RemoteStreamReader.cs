namespace AppStoreIntegrationServiceManagement.Model.Plugins
{
    public class RemoteStreamReader
    {
        private readonly HttpClient _httpClient;
        private readonly HttpRequestMessage _httpRequestMessage;

        public RemoteStreamReader(Uri url)
        {
            _httpClient = new HttpClient();
            _httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = url
            };
        }

        public async Task<Stream> ReadAsync()
        {
            HttpResponseMessage downloadResponse;

            try
            {
                downloadResponse = await _httpClient.SendAsync(_httpRequestMessage);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            if (!downloadResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Error! {downloadResponse.StatusCode}");
            }

            return await downloadResponse.Content.ReadAsStreamAsync();
        }
    }
}
