// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Message
{
    internal class DeleteMessageAttachmentRequest : BaseRequest
    {
        public DeleteMessageAttachmentRequest(Guid messageId, Guid attachmentId)
        {
            MessageId = messageId;
            AttachmentId = attachmentId;
        }

        public Guid MessageId { get; set; }
        public Guid AttachmentId { get; set; }
    }
}
