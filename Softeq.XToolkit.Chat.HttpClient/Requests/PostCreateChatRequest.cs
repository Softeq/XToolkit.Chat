// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PostCreateChatRequest : BasePostRestRequest<CreateChatDto>
    {
        private readonly string _apiUrl;

        public PostCreateChatRequest(
            string apiUrl,
            IJsonSerializer jsonSerializer,
            CreateChatDto dto)
            : base(jsonSerializer, dto)
        {
            _apiUrl = apiUrl;
        }

        public override string EndpointUrl => $"{_apiUrl}/channel";

        public override bool UseOriginalEndpoint => true;
    }
}