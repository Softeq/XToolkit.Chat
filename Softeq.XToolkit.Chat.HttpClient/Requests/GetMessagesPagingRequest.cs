// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData.HttpClient;
using Softeq.XToolkit.RemoteData;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal abstract class GetMessagesPagingRequest : BaseRestRequest
    {
        private readonly string _queryParams;

        public GetMessagesPagingRequest(GetMessagesQuery query)
        {
            ChannelId = query.ChannelId;

            var queryBuilder = new QueryStringBuilder()
                .AddParam("messageId", query.FromId);

            if (query.FromDateTime.HasValue)
            {
                var messageCreated = query.FromDateTime.Value.ToString("u");
                queryBuilder.AddParam("messageCreated", messageCreated);
            }

            if (query.Count.HasValue)
            {
                queryBuilder.AddParam("pageSize", query.Count);
            }

            _queryParams = queryBuilder.Build();
        }

        protected string ChannelId { get; }

        public sealed override string EndpointUrl => MainEndpointUrl + _queryParams;

        public override bool UseOriginalEndpoint => true;

        protected abstract string MainEndpointUrl { get; }
    }
}
