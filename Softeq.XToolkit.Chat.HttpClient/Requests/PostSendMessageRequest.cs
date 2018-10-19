// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PostSendMessageRequest : BasePostRestRequest<SendMessageDto>
    {
        private readonly string _apiUrl;
        private readonly string _channelId;

        public PostSendMessageRequest(
            string apiUrl,
            string channelId,
            IJsonSerializer jsonSerializer,
            SendMessageDto dto)
            : base(jsonSerializer, dto)
        {
            _apiUrl = apiUrl;
            _channelId = channelId;
        }

        public override string EndpointUrl => $"{_apiUrl}/channel/{_channelId}/message";

        public override bool UseOriginalEndpoint => true;
    }
}