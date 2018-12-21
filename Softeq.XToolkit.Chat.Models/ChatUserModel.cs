// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.Models
{
    public class ChatUserModel
    {
        public string Id { get; set; }
        public string SaasUserId { get; set; }
        public string Username { get; set; }
        public string PhotoUrl { get; set; }
        public bool IsOnline { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset LastActivity { get; set; }
    }
}
