// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Net.Http;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PostMarkAsReadRequest : BaseRestRequest
    {
        private readonly string _channelId;
        private readonly string _messageId;

        public PostMarkAsReadRequest(string channelId, string messageId)
        {
            _channelId = channelId;
            _messageId = messageId;
        }

        public override HttpMethod Method => HttpMethod.Post;

        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/channel/{_channelId}/message/{_messageId}/mark-as-read";

        public override bool UseOriginalEndpoint => true;
    }
}