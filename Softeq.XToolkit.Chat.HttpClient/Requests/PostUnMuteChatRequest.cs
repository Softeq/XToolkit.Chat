// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using System.Net.Http;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal sealed class PostUnMuteChatRequest : BaseRestRequest
    {
        public PostUnMuteChatRequest(string apiUrl, string chatId)
        {
            EndpointUrl = $"{apiUrl}/channel/{chatId}/unmute";
        }

        public override HttpMethod Method => HttpMethod.Post;

        public override string EndpointUrl { get; }

        public override bool UseOriginalEndpoint => true;
    }
}
