// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Message
{
    internal class CreateMessageRequest : BaseRequest
    {
        public string SaasUserId { get; set; } // TODO: check using
        public Guid ChannelId { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
    }
}
