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
        public bool IsClosed { get; set; }
        public bool IsMuted { get; set; }
        public bool IsPinned { get; set; }
        public string CreatorId { get; set; }
        public ChatUserModel DirectMember { get; set; }
        public string Description { get; set; }
        public string WelcomeMessage { get; set; }
        public ChannelType Type { get; set; }
        public ChatMessageModel LastMessage { get; set; }
        public string PhotoUrl { get; set; }

        public IList<string> TypingUsersNames { get; set; }
        public bool AreMoreThanThreeUsersTyping { get; set; }
        public bool IsCreatedByMe { get; private set; }

        // TODO YP: move to backend side
        public void UpdateIsCreatedByMeStatus(string currentUserId)
        {
            IsCreatedByMe = CreatorId == currentUserId;

            UpdateModelByType();
        }

        public void UpdateModelByType()
        {
            if (Type == ChannelType.Direct && DirectMember != null)
            {
                Name = DirectMember.Username;
                PhotoUrl = DirectMember.PhotoUrl;

                IsCreatedByMe = false;
            }
        }
    }
}