// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetChatsListRequest : BaseRestRequest
    {
        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/channel/allowed";

        public override bool UseOriginalEndpoint => true;
    }
}