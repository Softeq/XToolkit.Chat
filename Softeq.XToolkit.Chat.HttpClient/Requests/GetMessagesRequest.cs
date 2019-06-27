// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetMessagesRequest : GetMessagesPagingRequest
    {
        private readonly string _apiUrl;

        public GetMessagesRequest(string apiUrl, GetMessagesQuery query) : base(query)
        {
            _apiUrl = apiUrl;
        }

        protected override string MainEndpointUrl => $"{_apiUrl}/channel/{ChannelId}/message";

        public override bool UseOriginalEndpoint => true;
    }
}
