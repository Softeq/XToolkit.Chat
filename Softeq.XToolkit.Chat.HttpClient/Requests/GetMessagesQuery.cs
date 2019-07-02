// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class GetMessagesQuery
    {
        public string ChannelId { get; set; }
        public string FromId { get; set; } = null;
        public DateTimeOffset? FromDateTime { get; set; } = null;
        public int? Count { get; set; } = null;
    }
}
