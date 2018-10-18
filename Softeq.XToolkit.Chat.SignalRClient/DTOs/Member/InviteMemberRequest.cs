// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Member
{
    internal class InviteMemberRequest : BaseRequest
    {
        public Guid ChannelId { get; set; }
        public Guid MemberId { get; set; }
    }
}
