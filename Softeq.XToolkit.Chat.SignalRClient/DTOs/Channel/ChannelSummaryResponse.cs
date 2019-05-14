// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Message;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Member;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel
{
    internal class ChannelSummaryResponse
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public int UnreadMessagesCount { get; set; }
        public string Name { get; set; }
        public bool IsClosed { get; set; }
        public bool IsMuted { get; set; }
        public bool IsPinned { get; set; }
        public string Description { get; set; }
        public string WelcomeMessage { get; set; }
        public int Type { get; set; }
        public MessageResponse LastMessage { get; set; }
        public string PhotoUrl { get; set; }
        public MemberSummary Creator { get; set; }
        public MemberSummary DirectMember { get; set; }
    }
}
