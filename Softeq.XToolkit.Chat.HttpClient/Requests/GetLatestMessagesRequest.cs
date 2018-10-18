// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.RemoteData.HttpClient;
using System;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetLatestMessagesRequest : BaseRestRequest
    {
        private readonly string _channelId;

        public GetLatestMessagesRequest(string channelId)
        {
            _channelId = channelId;
        }

        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/channel/{_channelId}/message/last";

        public override bool UseOriginalEndpoint => true;
    }
}
