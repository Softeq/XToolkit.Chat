﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.HttpClient.Dtos
{
    internal class ChatSummaryDto
    {
        public string Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public int UnreadMessagesCount { get; set; }
        public string Name { get; set; }
        public bool IsClosed { get; set; }
        public bool IsMuted { get; set; }
        public bool IsPinned { get; set; }
        public ChatUserDto Creator { get; set; }
        public ChatUserDto DirectMember { get; set; }
        public string Description { get; set; }
        public string WelcomeMessage { get; set; }
        public int Type { get; set; }
        public ChatMessageDto LastMessage { get; set; }
        public string PhotoUrl { get; set; }
    }
}