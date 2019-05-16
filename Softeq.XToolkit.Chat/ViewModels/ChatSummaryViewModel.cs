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
    // TODO YP: split viewmodel to separate viewmodels for chat list and chat messages
    public class ChatSummaryViewModel : ObservableObject,
        IViewModelParameter<ChatSummaryModel>,
        IEquatable<ChatSummaryViewModel>
    {
        private const string TypingUsersDelimiter = ",";
        private const string SpaceDelimiter = " ";
        private const int MaxVisibleTypingUsersCount = 3;

        private readonly IFormatService _formatService;

        private ChatSummaryModel _chatSummary;

        public ChatSummaryViewModel(
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService)
        {
            LocalizedStrings = localizedStrings;
            _formatService = formatService;
        }

        public ChatSummaryModel Parameter
        {
            get => _chatSummary;
            set
            {
                _chatSummary = value ?? new ChatSummaryModel();

                LastMessageViewModel = new LastMessageBodyViewModel(_formatService, _chatSummary.LastMessage);
            }
        }

        public string ChatId => _chatSummary.Id;
        public string CreatorId => _chatSummary.CreatorId;
        public string ChatName => _chatSummary.Name;

        public LastMessageBodyViewModel LastMessageViewModel { get; private set; }

        public IChatLocalizedStrings LocalizedStrings { get; }

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

        public string ChatPhotoUrl
        {
            get => _chatSummary.PhotoUrl;
            set
            {
                _chatSummary.PhotoUrl = value;
                RaisePropertyChanged();
            }
        }

        public bool IsMuted => _chatSummary.IsMuted;
        public bool IsCreatedByMe => _chatSummary.IsCreatedByMe;
        public bool IsDirect => _chatSummary.Type == ChannelType.Direct;

        public bool CanLeave => !IsDirect;
        public bool CanClose => IsCreatedByMe || IsDirect;

        public DateTimeOffset LastUpdateDate => GetLastUpdateDate();

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
                    result += TypingUsersDelimiter + SpaceDelimiter + TypingUsersNames[i];
                }
                if (AreMoreThanThreeUsersTyping)
                {
                    result += SpaceDelimiter + LocalizedStrings.AndOther;
                }
                result += SpaceDelimiter + _formatService.PluralizeWithQuantity(TypingUsersNames.Count,
                                                                                LocalizedStrings.TypingPlural,
                                                                                LocalizedStrings.TypingSingular);
                return result;
            }
        }

        public void UpdateLastMessage(ChatMessageModel newLastMessage)
        {
            _chatSummary.LastMessage = newLastMessage;

            LastMessageViewModel.UpdateModel(newLastMessage);
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

        private DateTimeOffset GetLastUpdateDate()
        {
            if (_chatSummary.LastMessage != null)
            {
                return _chatSummary.LastMessage.DateTime;
            }

            return _chatSummary.UpdatedDate ?? _chatSummary.CreatedDate;
        }
    }
}
