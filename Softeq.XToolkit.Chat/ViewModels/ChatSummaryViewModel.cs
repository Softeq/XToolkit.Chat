// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatSummaryViewModel : ViewModelBase, IViewModelParameter<ChatSummaryModel>, IEquatable<ChatSummaryViewModel>
    {
        private const int MaxVisibleTypingUsersCount = 3;
        private readonly ISocketChatAdapter _chatAdapter;
        private ChatSummaryModel _chatSummary;

        public ChatSummaryViewModel(ISocketChatAdapter chatAdapter)
        {
            _chatAdapter = chatAdapter;
        }

        public ChatSummaryModel Parameter
        {
            set => _chatSummary = value ?? new ChatSummaryModel();
        }

        public string ChatId => _chatSummary.Id;
        public string ChatName => _chatSummary.Name;
        public string LastMessageUsername => _chatSummary.LastMessage?.SenderName;
        public string LastMessageBody => _chatSummary.LastMessage?.Body;
        public ChatMessageStatus LastMessageStatus => _chatSummary.LastMessage?.Status ?? ChatMessageStatus.Other;
        public string LastMessageDateTime => _chatSummary.LastMessage?.DateTime.LocalDateTime.ToString("HH:mm");

        public int UnreadMessageCount
        {
            get => _chatSummary.UnreadMessagesCount;
            set
            {
                if (_chatSummary.UnreadMessagesCount != value)
                {
                    _chatSummary.UnreadMessagesCount = value;
                    Execute.BeginOnUIThread(() => RaisePropertyChanged());
                }
            }
        }

        public string ChatPhotoUrl => _chatSummary.AvatarUrl;
        public bool IsMuted => _chatSummary.IsMuted;
        public bool IsCreatedByMe => _chatSummary.IsCreatedByMe;

        public DateTimeOffset LastUpdateDate => _chatSummary.LastMessage != null
                                                            ? _chatSummary.LastMessage.DateTime
                                                            : _chatSummary.UpdatedDate
                                                            ?? _chatSummary.CreatedDate;

        public IList<string> TypingUsersNames
        {
            get => _chatSummary.TypingUsersNames;
            set
            {
                _chatSummary.TypingUsersNames = value;
                RaisePropertyChanged(nameof(TypingUsersNamesText));
            }
        }

        public bool AreMoreThanThreeUsersTyping
        {
            get => _chatSummary.AreMoreThanThreeUsersTyping;
            set
            {
                _chatSummary.AreMoreThanThreeUsersTyping = value;
                RaisePropertyChanged(nameof(TypingUsersNamesText));
            }
        }

        public string TypingUsersNamesText
        {
            get
            {
                if (TypingUsersNames == null || !TypingUsersNames.Any())
                {
                    return string.Empty;
                }
                var result = TypingUsersNames[0];
                for (int i = 0; i < MaxVisibleTypingUsersCount; i++)
                {
                    result += $", {TypingUsersNames[i]}";
                }
                if (AreMoreThanThreeUsersTyping)
                {
                    result += " and other";
                }
                result += TypingUsersNames.Count > 1 ? " are typing..." : " is typing...";
                return result;
            }
        }

        public void UpdateLastMessage(ChatMessageModel newLastMessage)
        {
            _chatSummary.LastMessage = newLastMessage;
            Execute.BeginOnUIThread(() =>
            {
                RaisePropertyChanged(nameof(LastMessageUsername));
                RaisePropertyChanged(nameof(LastMessageBody));
                RaisePropertyChanged(nameof(LastMessageStatus));
                RaisePropertyChanged(nameof(LastMessageDateTime));
            });
        }

        public bool Equals(ChatSummaryViewModel other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return ChatId == other.ChatId;
        }

        public override bool Equals(object obj) => Equals(obj as ChatSummaryViewModel);

        public override int GetHashCode()
        {
            return ChatId == null ? 0 : ChatId.GetHashCode();
        }
    }
}
