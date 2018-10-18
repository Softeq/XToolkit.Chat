// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel
{
    internal class UpdateChannelRequest : BaseRequest
    {
        public string SaasUserId { get; set; }
        public Guid ChannelId { get; set; }
        public string Name { get; set; }
        public string Topic { get; set; }
        public string WelcomeMessage { get; set; }
        // Private channel
        public ChannelTypeDto Type { get; set; }
        public List<Guid> AllowedMembers { get; set; }
    }
}
