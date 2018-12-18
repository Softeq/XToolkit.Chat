// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.RemoteData.HttpClient;
using Softeq.XToolkit.RemoteData;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal abstract class GetMessagesPagingRequest : BaseRestRequest
    {
        private readonly string _queryParams;

        public GetMessagesPagingRequest(
            string channelId,
            string messageFromId = null,
            DateTimeOffset? messageFromDateTime = null,
            int? pageSize = null)
        {
            ChannelId = channelId;

            var messageCreated = messageFromDateTime.Value.ToString("u");

            _queryParams = new QueryStringBuilder()
                .AddParam("messageId", messageFromId)
                .AddParam("messageCreated", messageCreated)
                .AddParam("pageSize", pageSize)
                .Build();
        }

        protected string ChannelId { get; }

        public sealed override string EndpointUrl => MainEndpointUrl + _queryParams;

        public override bool UseOriginalEndpoint => true;

        protected abstract string MainEndpointUrl { get; }
    }
}
