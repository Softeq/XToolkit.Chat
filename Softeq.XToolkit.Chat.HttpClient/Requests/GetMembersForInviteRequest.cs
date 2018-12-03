// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.RemoteData;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetMembersForInviteRequest : BaseRestRequest
    {
        public GetMembersForInviteRequest(
            string apiUrl,
            string chatId,
            string nameFilter,
            int pageNumber,
            int pageSize)
        {
            var queryParams = new QueryStringBuilder()
                .AddParam("pageNumber", pageNumber.ToString())
                .AddParam("pageSize", pageSize.ToString())
                .AddParam("nameFilter", nameFilter)
                .Build();

            EndpointUrl = $"{apiUrl}/channel/{chatId}/invite/user{queryParams}";
        }

        public override string EndpointUrl { get; }

        public override bool UseOriginalEndpoint => true;
    }
}