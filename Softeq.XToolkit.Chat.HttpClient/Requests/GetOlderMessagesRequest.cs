// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.Models;
using System;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetOlderMessagesRequest : GetMessagesPagingRequest
    {
        public GetOlderMessagesRequest(string channelId,
                                       string messageFromId = null,
                                       DateTimeOffset? messageFromDateTime = null,
                                       int? pageSize = null) : base(channelId, messageFromId, messageFromDateTime, pageSize) { }

        protected override string MainEndpointUrl => $"{ChatConfig.ApiUrl}/channel/{ChannelId}/message/old";

        public override bool UseOriginalEndpoint => true;
    }
}
