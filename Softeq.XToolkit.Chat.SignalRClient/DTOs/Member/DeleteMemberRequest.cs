using System;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Member
{
    internal class DeleteMemberRequest : BaseRequest
    {
        public Guid ChannelId { get; set; }
        public Guid MemberId { get; set; }
    }
}
