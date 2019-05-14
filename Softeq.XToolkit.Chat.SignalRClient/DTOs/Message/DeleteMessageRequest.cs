// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Message
{
    internal class DeleteMessageRequest : BaseRequest
    {
        public Guid MessageId { get; set; }
    }
}
