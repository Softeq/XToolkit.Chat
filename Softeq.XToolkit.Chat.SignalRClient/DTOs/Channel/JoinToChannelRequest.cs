// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel
{
    internal class JoinToChannelRequest : BaseRequest
    {
        public Guid ChannelId { get; set; }
    }
}
