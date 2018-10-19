// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetMembersRequest : BaseRestRequest
    {
        private readonly string _apiUrl;

        public GetMembersRequest(string apiUrl)
        {
            _apiUrl = apiUrl;
        }

        public override string EndpointUrl => $"{_apiUrl}/member";

        public override bool UseOriginalEndpoint => true;
    }
}