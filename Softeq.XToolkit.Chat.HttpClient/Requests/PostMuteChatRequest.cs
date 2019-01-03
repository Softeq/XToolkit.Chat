using System.Net.Http;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal sealed class PostMuteChatRequest : BaseRestRequest
    {
        public PostMuteChatRequest(string apiUrl, string chatId)
        {
            EndpointUrl = $"{apiUrl}/channel/{chatId}/mute";
        }

        public override HttpMethod Method => HttpMethod.Post;

        public override string EndpointUrl { get; }

        public override bool UseOriginalEndpoint => true;
    }
}
