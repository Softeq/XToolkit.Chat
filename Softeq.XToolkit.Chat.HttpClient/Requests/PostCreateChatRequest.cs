// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PostCreateChatRequest : BasePostRestRequest<CreateChatDto>
    {
        public PostCreateChatRequest(IJsonSerializer jsonSerializer, CreateChatDto dto) : base(jsonSerializer, dto) { }

        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/channel";

        public override bool UseOriginalEndpoint => true;
    }
}