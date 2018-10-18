// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.IO;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Message
{
    internal class AddMessageAttachmentRequest : BaseRequest
    {
        public AddMessageAttachmentRequest(Guid messageId, Stream content, string extension, string contentType, long size)
        {
            MessageId = messageId;
            Content = content;
            Extension = extension;
            ContentType = contentType;
            Size = size;
        }

        public Guid MessageId { get; set; }
        public Stream Content { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
    }
}
