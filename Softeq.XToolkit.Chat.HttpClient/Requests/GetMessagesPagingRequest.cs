// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal abstract class GetMessagesPagingRequest : BaseRestRequest
    {
        private readonly string _messageFromId;
        private readonly DateTimeOffset? _messageFromDateTime;
        private readonly int? _pageSize;

        public GetMessagesPagingRequest(string channelId,
                                        string messageFromId = null,
                                        DateTimeOffset? messageFromDateTime = null,
                                        int? pageSize = null)
        {
            ChannelId = channelId;
            _messageFromId = messageFromId;
            _messageFromDateTime = messageFromDateTime;
            _pageSize = pageSize;
        }

        protected string ChannelId { get; }

        public sealed override string EndpointUrl
        {
            get
            {
                var messageCreated = _messageFromDateTime.Value.ToString("u");

                var urlParams = _messageFromId != null ? $"messageId={_messageFromId}&" : string.Empty;
                urlParams += _messageFromDateTime.HasValue ? $"messageCreated={messageCreated}&" : string.Empty;
                urlParams += _pageSize.HasValue ? $"pageSize={_pageSize.Value}" : string.Empty;
                return string.IsNullOrEmpty(urlParams) ? MainEndpointUrl : $"{MainEndpointUrl}?{urlParams.Trim('&')}";
            }
        }

        public override bool UseOriginalEndpoint => true;

        protected abstract string MainEndpointUrl { get; }
    }
}
