// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetChatMembersRequest : BaseRestRequest
    {
        private readonly string _apiUrl;
        private readonly string _chatId;

        public GetChatMembersRequest(string apiUrl, string chatId)
        {
            _apiUrl = apiUrl;
            _chatId = chatId;
        }

        public override string EndpointUrl => $"{_apiUrl}/channel/{_chatId}/participant";

        public override bool UseOriginalEndpoint => true;
    }
}
