// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.HttpClient.Dtos
{
    internal class ChatMessageDto
    {
        public Guid Id { get; set; }
        public string ChannelId { get; set; }
        public ChatUserDto Sender { get; set; }
        public string Body { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public MessageTypeDto Type { get; set; }
        public bool IsRead { get; set; }
        public string ImageUrl { get; set; }
        public int ChannelType { get; set; }
    }
}
