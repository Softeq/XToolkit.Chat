// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Member;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Message
{
    internal class MessageResponse
    {
        public Guid Id { get; set; }
        public Guid ChannelId { get; set; }
        public MemberSummary Sender { get; set; }
        public string Body { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public MessageType Type { get; set; }
        public bool IsRead { get; set; }
        public string ImageUrl { get; set; }
        public int ChannelType { get; set; }
    }
}
