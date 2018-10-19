﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Net.Http;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PutCloseChatRequest : BaseRestRequest
    {
        private readonly string _apiUrl;
        private readonly string _chatId;

        public PutCloseChatRequest(string apiUrl, string chatId)
        {
            _apiUrl = apiUrl;
            _chatId = chatId;
        }

        public override string EndpointUrl => $"{_apiUrl}/channel/{_chatId}/close";

        public override HttpMethod Method => HttpMethod.Put;

        public override bool UseOriginalEndpoint => true;
    }
}