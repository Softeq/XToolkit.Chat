// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;

namespace Softeq.XToolkit.Chat.Models
{
    public class ChatSummaryModel
    {
		public string Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? UpdatedDate { get; set; }
		public string Name { get; set; }
        public int UnreadMessagesCount { get; set; }
		public bool IsMuted { get; set; }
        public string CreatorId { get; set; }
        public bool IsCreatedByMe { get; private set; }
		public string AvatarUrl { get; set; }
        public ChatMessageModel LastMessage { get; set; }
        public IList<string> TypingUsersNames { get; set; }
        public bool AreMoreThanThreeUsersTyping { get; set; }
        public string Topic { get; set; }
        public string WelcomeMessage { get; set; }

        public void UpdateIsCreatedByMeStatus(string currentUserId)
        {
            IsCreatedByMe = CreatorId == currentUserId;
        }
    }
}
