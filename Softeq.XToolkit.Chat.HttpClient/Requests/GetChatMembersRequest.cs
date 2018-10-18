// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetChatMembersRequest : BaseRestRequest
    {
        public readonly string _chatId;

        public GetChatMembersRequest(string chatId)
        {
            _chatId = chatId;
        }

        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/channel/{_chatId}/participant";

        public override bool UseOriginalEndpoint => true;
    }
}
