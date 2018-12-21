// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.HttpClient.Dtos
{
    internal class ChatUserDto
    {
        public string Id { get; set; }
        public string SaasUserId { get; set; }
        public string UserName { get; set; }
        public ChatUserRoleDto Role { get; set; }
        public ChatUserStatusDto Status { get; set; }
        public bool IsAfk { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset LastActivity { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }
}
