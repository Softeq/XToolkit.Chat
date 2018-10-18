// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Message
{
    internal class UpdateMessageRequest : BaseRequest
    {
        public string SaasUserId { get; set; }
        public Guid MessageId { get; set; }
        public string Body { get; set; }
    }
}
