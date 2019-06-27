// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Message;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PostMarkAsReadRequest : BasePostRestRequest<SetLastReadMessageRequest>
    {
        public PostMarkAsReadRequest(
            string apiUrl,
            IJsonSerializer jsonSerializer,
            SetLastReadMessageRequest dto)
            : base(jsonSerializer, dto)
        {
            EndpointUrl = $"{apiUrl}/channel/{dto.ChannelId}/message/{dto.MessageId}/mark-as-read";
        }

        public override string EndpointUrl { get; }

        public override bool UseOriginalEndpoint => true;
    }
}