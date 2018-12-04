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
    public class ChatSummaryViewModel : ViewModelBase,
        IViewModelParameter<ChatSummaryModel>,
        IEquatable<ChatSummaryViewModel>
    {
        private const string TypingUsersDelimiter = ",";
        private const string SpaceDelimiter = " ";
        private const int MaxVisibleTypingUsersCount = 3;

        private readonly ISocketChatAdapter _chatAdapter;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly IFormatService _formatService;

        public ChatSummaryViewModel(
            ISocketChatAdapter chatAdapter,
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService)
        {
            _chatAdapter = chatAdapter;
            _localizedStrings = localizedStrings;
            _formatService = formatService;
        }

        public ChatSummaryModel Parameter
        {
            get => null;
            set => ChatSummary = value ?? new ChatSummaryModel();
        }

        public ChatSummaryModel ChatSummary { get; set; }

        public string ChatId => ChatSummary.Id;
        public string ChatName => ChatSummary.Name;
        public string LastMessageUsername => ChatSummary.LastMessage?.SenderName;
        public string LastMessageBody => ChatSummary.LastMessage?.Body;
        public ChatMessageStatus LastMessageStatus => ChatSummary.LastMessage?.Status ?? ChatMessageStatus.Other;
        public string LastMessageDateTime => _formatService.ToShortTimeFormat(ChatSummary.LastMessage?.DateTime.LocalDateTime);

        public int UnreadMessageCount
        {
            get => ChatSummary.UnreadMessagesCount;
            set
            {
                if (ChatSummary.UnreadMessagesCount != value)
                {
                    ChatSummary.UnreadMessagesCount = value;
                    Execute.BeginOnUIThread(() => RaisePropertyChanged());
                }
            }
        }

        public string ChatPhotoUrl
        {
            get => ChatSummary.AvatarUrl;
            set
            {
                ChatSummary.AvatarUrl = value;
                RaisePropertyChanged();
            }
        }

        public bool IsMuted => ChatSummary.IsMuted;
        public bool IsCreatedByMe => ChatSummary.IsCreatedByMe;

        public DateTimeOffset LastUpdateDate => ChatSummary.LastMessage != null
                                                            ? ChatSummary.LastMessage.DateTime
                                                            : ChatSummary.UpdatedDate
                                                            ?? ChatSummary.CreatedDate;

        public IList<string> TypingUsersNames
        {
            get => ChatSummary.TypingUsersNames;
            set
            {
                ChatSummary.TypingUsersNames = value;
                RaisePropertyChanged(nameof(TypingUsersNamesText));
            }
        }

        public bool AreMoreThanThreeUsersTyping
        {
            get => ChatSummary.AreMoreThanThreeUsersTyping;
            set
            {
                ChatSummary.AreMoreThanThreeUsersTyping = value;
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
                    result += TypingUsersDelimiter + SpaceDelimiter + TypingUsersNames[i];
                }
                if (AreMoreThanThreeUsersTyping)
                {
                    result += SpaceDelimiter + _localizedStrings.AndOther;
                }
                result += SpaceDelimiter + _formatService.PluralizeWithQuantity(TypingUsersNames.Count,
                                                                                _localizedStrings.TypingPlural,
                                                                                _localizedStrings.TypingSingular);
                return result;
            }
        }

        public void UpdateLastMessage(ChatMessageModel newLastMessage)
        {
            ChatSummary.LastMessage = newLastMessage;
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
