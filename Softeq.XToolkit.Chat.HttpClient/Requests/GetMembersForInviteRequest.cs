// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Member;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetMembersForInviteRequest : BasePostRestRequest<GetPotentialChannelMembersRequest>
    {
        public GetMembersForInviteRequest(
            string apiUrl,
            IJsonSerializer jsonSerializer,
            GetPotentialChannelMembersRequest dto,
            string channelId)
            : base(jsonSerializer, dto)
        {
            EndpointUrl = $"{apiUrl}/channel/{channelId}/invite/user";
        }

        public override bool UseOriginalEndpoint => true;

        public override string EndpointUrl { get; }
    }
}