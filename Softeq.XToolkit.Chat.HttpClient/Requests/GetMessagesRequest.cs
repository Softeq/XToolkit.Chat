// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.Models;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetMessagesRequest : GetMessagesPagingRequest
    {
        public GetMessagesRequest(string channelId,
                                  string messageFromId = null,
                                  DateTimeOffset? messageFromDateTime = null,
                                  int? pageSize = null) : base(channelId, messageFromId, messageFromDateTime, pageSize) { }

        protected override string MainEndpointUrl => $"{ChatConfig.ApiUrl}/channel/{ChannelId}/message";

        public override bool UseOriginalEndpoint => true;
    }
}
