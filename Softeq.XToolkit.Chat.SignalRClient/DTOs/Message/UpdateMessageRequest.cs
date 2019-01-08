// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Message
{
    internal class UpdateMessageRequest : BaseRequest
    {
        public Guid MessageId { get; set; }
        public string Body { get; set; }
    }
}
