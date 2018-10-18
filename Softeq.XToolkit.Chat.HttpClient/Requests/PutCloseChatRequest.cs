// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Net.Http;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class PutCloseChatRequest : BaseRestRequest
    {
        private readonly string _chatId;

        public PutCloseChatRequest(string chatId)
        {
            _chatId = chatId;
        }

        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/channel/{_chatId}/close";

        public override HttpMethod Method => HttpMethod.Put;

        public override bool UseOriginalEndpoint => true;
    }
}