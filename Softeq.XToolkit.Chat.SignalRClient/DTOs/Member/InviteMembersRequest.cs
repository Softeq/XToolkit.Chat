// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Member
{
    internal class InviteMembersRequest : BaseRequest
    {
        public Guid ChannelId { get; set; }
        public IList<string> InvitedMembersIds { get; set; }
    }
}
