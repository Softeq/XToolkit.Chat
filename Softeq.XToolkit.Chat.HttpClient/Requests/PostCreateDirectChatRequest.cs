// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Channel;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PostCreateDirectChatRequest : BasePostRestRequest<CreateDirectChannelRequest>
    {
        private readonly string _apiUrl;

        public PostCreateDirectChatRequest(
            string apiUrl,
            IJsonSerializer jsonSerializer,
            CreateDirectChannelRequest dto)
            : base(jsonSerializer, dto)
        {
            _apiUrl = apiUrl;
        }

        public override string EndpointUrl => $"{_apiUrl}/channel/direct";

        public override bool UseOriginalEndpoint => true;
    }
}
