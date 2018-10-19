// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetChatsListRequest : BaseRestRequest
    {
        private readonly string _apiUrl;

        public GetChatsListRequest(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public override string EndpointUrl => $"{_apiUrl}/channel/allowed";

        public override bool UseOriginalEndpoint => true;
    }
}