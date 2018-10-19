// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetUserSummaryRequest : BaseRestRequest
    {
        private readonly string _apiUrl;

        public GetUserSummaryRequest(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public override string EndpointUrl => $"{_apiUrl}/me/member";

        public override bool UseOriginalEndpoint => true;
    }
}