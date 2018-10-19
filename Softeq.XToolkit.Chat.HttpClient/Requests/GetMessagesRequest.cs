// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetMessagesRequest : GetMessagesPagingRequest
    {
        private readonly string _apiUrl;

        public GetMessagesRequest(
            string apiUrl,
            string channelId,
            string messageFromId = null,
            DateTimeOffset? messageFromDateTime = null,
            int? pageSize = null)
            : base(channelId, messageFromId, messageFromDateTime, pageSize)
        {
            _apiUrl = apiUrl;
        }

        protected override string MainEndpointUrl => $"{_apiUrl}/channel/{ChannelId}/message";

        public override bool UseOriginalEndpoint => true;
    }
}
