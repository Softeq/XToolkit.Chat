// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData.HttpClient;
using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.Chat.Models;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PostSendMessageRequest : BasePostRestRequest<SendMessageDto>
    {
        private readonly string _channelId;

        public PostSendMessageRequest(string channelId, IJsonSerializer jsonSerializer, SendMessageDto dto) : base(jsonSerializer, dto)
        {
            _channelId = channelId;
        }

        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/channel/{_channelId}/message";

        public override bool UseOriginalEndpoint => true;
    }
}