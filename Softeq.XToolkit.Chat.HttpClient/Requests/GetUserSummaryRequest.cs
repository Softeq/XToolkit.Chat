// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetUserSummaryRequest : BaseRestRequest
    {
        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/me/member";

        public override bool UseOriginalEndpoint => true;
    }
}