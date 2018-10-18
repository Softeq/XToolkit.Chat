// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Message
{
    internal class DeleteMessageRequest : BaseRequest
    {
        public Guid ChannelId { get; set; }
        public Guid MessageId { get; set; }
    }
}
