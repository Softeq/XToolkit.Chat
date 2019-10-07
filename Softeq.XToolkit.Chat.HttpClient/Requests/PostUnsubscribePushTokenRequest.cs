﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Net.Http;
using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    public class PostUnsubscribePushTokenRequest : BasePostRestRequest<PushTokenDto>
    {
        private readonly string _apiUrl;

        public PostUnsubscribePushTokenRequest(string apiUrl,
            IJsonSerializer jsonSerializer,
            PushTokenDto dto)
            : base(jsonSerializer, dto)
        {
            _apiUrl = apiUrl;
        }

        public override HttpMethod Method => HttpMethod.Post;

        public override string EndpointUrl => $"{_apiUrl}/member/push-token/unsubscribe";

        public override bool UseOriginalEndpoint => true;
    }
}
