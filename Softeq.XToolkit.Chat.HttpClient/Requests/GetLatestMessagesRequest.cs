// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetLatestMessagesRequest : BaseRestRequest
    {
        private readonly string _apiUrl;
        private readonly string _channelId;

        public GetLatestMessagesRequest(string apiUrl, string channelId)
        {
            _apiUrl = apiUrl;
            _channelId = channelId;
        }

        public override string EndpointUrl => $"{_apiUrl}/channel/{_channelId}/message/last";

        public override bool UseOriginalEndpoint => true;
    }
}
