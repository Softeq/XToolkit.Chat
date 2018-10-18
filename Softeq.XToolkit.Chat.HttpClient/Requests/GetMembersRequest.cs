// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetMembersRequest : BaseRestRequest
    {
        public override string EndpointUrl => $"{ChatConfig.ApiUrl}/member";

        public override bool UseOriginalEndpoint => true;
    }
}