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

        /// <summary>
        /// Must be personalized for current user
        /// </summary>
        public ChatUserModel Member { get; set; }

        /// <summary>
        /// Only for direct channels
        /// </summary>
        public ChatUserModel DirectMember { get; set; }

        public string Description { get; set; }
        public string WelcomeMessage { get; set; }
        public ChannelType Type { get; set; }
        public ChatMessageModel LastMessage { get; set; }
        public string PhotoUrl { get; set; }

        public IList<string> TypingUsersNames { get; set; }
        public bool AreMoreThanThreeUsersTyping { get; set; }
        public bool IsCreatedByMe { get; private set; }

        public void UpdateIsCreatedByMeStatus(string currentUserId)
        {
            // TODO YP: Check of Id can be removed after fix personalized member on backend side
            IsCreatedByMe = Member.Id == currentUserId
                && Member.Role == Enum.UserRole.Admin;

            UpdateModelByType();
        }

        private void UpdateModelByType()
        {
            // TODO YP: move to backend
            // - different Creator & DirectMemer models for Direct chats
            //   depends on chat creator user
            // - currently each channel user received the same event about new channel,
            //   need to send two different event about this
            if (Type == ChannelType.Direct)
            {
                if (IsCreatedByMe && DirectMember != null)
                {
                    Name = DirectMember.Username;
                    PhotoUrl = DirectMember.PhotoUrl;
                }
                else if (Member != null)
                {
                    Name = Member.Username;
                    PhotoUrl = Member.PhotoUrl;
                }

                IsCreatedByMe = false;
            }
        }
    }
}