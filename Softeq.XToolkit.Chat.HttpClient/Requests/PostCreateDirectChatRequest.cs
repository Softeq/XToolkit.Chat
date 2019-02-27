using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PostCreateDirectChatRequest : BasePostRestRequest<CreateDirectChatDto>
    {
        private readonly string _apiUrl;

        public PostCreateDirectChatRequest(
            string apiUrl,
            IJsonSerializer jsonSerializer,
            CreateDirectChatDto dto)
            : base(jsonSerializer, dto)
        {
            _apiUrl = apiUrl;
        }

        public override string EndpointUrl => $"{_apiUrl}/channel/direct";

        public override bool UseOriginalEndpoint => true;
    }
}
