// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetAzureTokenRequest : BaseRestRequest
    {
        private readonly string _apiUrl;

        public GetAzureTokenRequest(string apiUrl)
        {
            _apiUrl = apiUrl;
        }
        
        public override string EndpointUrl => $"{_apiUrl}/file/get-access-token";
        
        public override bool UseOriginalEndpoint => true;
    }
}