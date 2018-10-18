// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Attachment
{
    internal class AttachmentResponse
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public Guid MessageId { get; set; }
    }
}
