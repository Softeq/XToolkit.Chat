// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.EventArguments;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.ImagePicker;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatMessagesViewModel : ViewModelBase, IViewModelParameter<ChatSummaryViewModel>
    {
        private const int InitialReadMessagesBatchCount = 20;
        private const int OlderMessagesBatchCount = 50;

        private readonly ChatManager _chatManager;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly IFormatService _formatService;
        private readonly IList<IDisposable> _subscriptions = new List<IDisposable>();

        private ChatSummaryViewModel _chatSummaryViewModel;
        private ChatMessageViewModel _messageBeingEdited;

        private bool _areLatestMessagesLoaded;
        private string _messageToSendBody = string.Empty;
        private bool _isInEditMessageMode;

        public ChatMessagesViewModel(
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService,
            ChatManager chatManager,
            ConnectionStatusViewModel connectionStatusViewModel)
        {
            _pageNavigationService = pageNavigationService;
            _localizedStrings = localizedStrings;
            _formatService = formatService;
            _chatManager = chatManager;

            ConnectionStatusViewModel = connectionStatusViewModel;

            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            SendCommand = new RelayCommand<GenericEventArgs<ImagePickerArgs>>(SendMessageAsync);
            CancelEditingMessageModeCommand = new RelayCommand(CancelEditingMessageMode);
            ShowInfoCommand = new RelayCommand(ShowInfo);
            LoadOlderMessagesCommand = new RelayCommand(() => LoadOlderMessagesAsync().SafeTaskWrapper());
        }

        public ChatSummaryViewModel Parameter
        {
            get => _chatSummaryViewModel;
            set
            {
                _chatSummaryViewModel = value;
                RaisePropertyChanged(nameof(ChatName));
            }
        }

        public ObservableKeyGroupsCollection<DateTimeOffset, ChatMessageViewModel> Messages { get; }
            = new ObservableKeyGroupsCollection<DateTimeOffset, ChatMessageViewModel>(message => message.DateTime.Date,
                (x, y) => x.CompareTo(y),
                (x, y) => x.DateTime.CompareTo(y.DateTime));

        public ConnectionStatusViewModel ConnectionStatusViewModel { get; }

        public string MessageToSendBody
        {
            get => _messageToSendBody;
            set => Set(ref _messageToSendBody, value);
        }

        public string ChatName => _chatSummaryViewModel?.ChatName;

        public string EditedMessageOriginalBody => _messageBeingEdited?.Body;

        public bool IsInEditMessageMode
        {
            get => _isInEditMessageMode;
            private set => Set(ref _isInEditMessageMode, value);
        }

        public string EditMessageLabelText => _localizedStrings.EditMessage;
        public string MessagePlaceholderText => _localizedStrings.YourMessage;

        public ICommand BackCommand { get; }
        public RelayCommand<GenericEventArgs<ImagePickerArgs>> SendCommand { get; }
        public ICommand CancelEditingMessageModeCommand { get; }
        public ICommand ShowInfoCommand { get; }
        public ICommand MessageAddedCommand { get; set; }
        public ICommand LoadOlderMessagesCommand { get; set; }

        public IReadOnlyList<CommandAction> MessageCommandActions => new List<CommandAction>
        {
            new CommandAction
            {
                Title = _localizedStrings.Edit,
                Command = new RelayCommand<ChatMessageViewModel>(EditMessage),
                CommandActionStyle = CommandActionStyle.Default
            },
            new CommandAction
            {
                Title = _localizedStrings.Delete,
                Command = new RelayCommand<ChatMessageViewModel>(DeleteMessage),
                CommandActionStyle = CommandActionStyle.Destructive
            }
        };

        public virtual string GetDateString(DateTimeOffset date)
        {
            return _formatService.Humanize(date, _localizedStrings.Today, _localizedStrings.Yesterday);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            _subscriptions.Add(_chatManager.MessageAdded.Subscribe(OnMessageReceived));
            _subscriptions.Add(_chatManager.MessageEdited.Subscribe(OnMessageEdited));
            _subscriptions.Add(_chatManager.MessageDeleted.Subscribe(OnMessageDeleted));
            _subscriptions.Add(_chatManager.MessagesBatchAdded.Subscribe(OnMessagesBatchReceived));
            _subscriptions.Add(_chatManager.MessagesBatchUpdated.Subscribe(OnMessagesBatchUpdated));
            _subscriptions.Add(_chatManager.MessagesBatchDeleted.Subscribe(OnMessagesBatchDeleted));

            ConnectionStatusViewModel.Initialize(ChatName);

            if (!_areLatestMessagesLoaded)
            {
                LoadInitialMessagesAsync().SafeTaskWrapper();
                _areLatestMessagesLoaded = true;
            }

            _chatManager.MakeChatActive(_chatSummaryViewModel?.ChatId);
            Messages.ItemsChanged += OnMessagesAddedToCollection;
            MarkMessagesAsRead();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();

            Messages.ItemsChanged -= OnMessagesAddedToCollection;
            _subscriptions.Apply(x => x.Dispose());

            ConnectionStatusViewModel.Dispose();
        }

        public async Task LoadOlderMessagesAsync()
        {
            var oldestMessage = Messages.FirstOrDefaultValue();
            if (oldestMessage == null)
            {
                await LoadInitialMessagesAsync();
                return;
            }
            var olderMessages = await _chatManager.LoadOlderMessagesAsync(
                _chatSummaryViewModel.ChatId,
                oldestMessage.Id,
                oldestMessage.DateTime,
                OlderMessagesBatchCount);
            Messages.AddRangeToGroupsSorted(olderMessages);
        }

        private async Task LoadInitialMessagesAsync()
        {
            var messages = await _chatManager.LoadInitialMessagesAsync(_chatSummaryViewModel.ChatId, InitialReadMessagesBatchCount);
            Messages.AddRangeToGroupsSorted(messages);
        }

        private void ShowInfo()
        {
            _pageNavigationService.For<ChatDetailsViewModel>()
                .WithParam(x => x.Summary, _chatSummaryViewModel.Parameter)
                .Navigate();
        }

        private void AttachImage()
        {
            _pageNavigationService.For<ChatDetailsViewModel>()
                .WithParam(x => x.Summary, _chatSummaryViewModel.Parameter)
                .Navigate();
        }

        private void ClearMessages()
        {
            _areLatestMessagesLoaded = false;
            Messages.ItemsChanged -= OnMessagesAddedToCollection;
            Messages.ClearAll();
        }

        private void OnMessagesAddedToCollection(object sender, NotifyKeyGroupsCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                MarkMessagesAsRead();
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                MarkMessagesAsRead();
            }
        }

        private void OnMessageReceived(ChatMessageViewModel chatMessage)
        {
            if (chatMessage.ChatId == _chatSummaryViewModel.ChatId)
            {
                AddNewMessages(new List<ChatMessageViewModel> { chatMessage });
            }
        }

        private void OnMessagesBatchReceived(IList<ChatMessageViewModel> messages)
        {
            var messagesToAdd = messages.Where(x => x.ChatId == _chatSummaryViewModel.ChatId).ToList();
            AddNewMessages(messagesToAdd);
        }

        private void AddNewMessages(IList<ChatMessageViewModel> messages)
        {
            Messages.AddRangeToGroupsSorted(messages);
            MessageAddedCommand?.Execute(null);
            MarkMessagesAsRead();
        }

        private void OnMessageEdited(ChatMessageModel messageModel)
        {
            bool WasUpdated(ChatMessageViewModel x) => x.Id == messageModel.Id;
            Messages.Where(x => x.Any(WasUpdated))
                    .SelectMany(x => x)
                    .Where(WasUpdated)
                    .Apply(x => x.Parameter = messageModel);
        }

        private void OnMessagesBatchUpdated(IList<ChatMessageModel> messagesModels)
        {
            foreach (var m in messagesModels)
            {
                OnMessageEdited(m);
            }
        }

        private void OnMessageDeleted(string deletedMessageId)
        {
            DeleteAllMessages(x => x.Id == deletedMessageId);
        }

        private void OnMessagesBatchDeleted(IList<string> deletedMessagesIds)
        {
            DeleteAllMessages(x => deletedMessagesIds.Contains(x.Id));
        }

        private void DeleteAllMessages(Func<ChatMessageViewModel, bool> predicate)
        {
            var messagesToDelete = Messages
                .Where(x => x.Any(predicate))
                .SelectMany(x => x)
                .Where(predicate)
                .ToList();
            Messages.RemoveAllFromGroups(messagesToDelete);
        }

        private async void SendMessageAsync(GenericEventArgs<ImagePickerArgs> e)
        {
            var photoSelector = e?.Value;
            var newMessageBody = MessageToSendBody?.Trim();

            if (photoSelector == null && string.IsNullOrEmpty(newMessageBody))
            {
                return;
            }

            MessageToSendBody = string.Empty;

            if (IsInEditMessageMode)
            {
                IsInEditMessageMode = false;

                await _chatManager.EditMessageAsync(_messageBeingEdited.Id, newMessageBody)
                    .ConfigureAwait(false);

                CancelEditingMessageMode();
            }
            else
            {
                await _chatManager.SendMessageAsync(_chatSummaryViewModel.ChatId, newMessageBody, e?.Value)
                    .ConfigureAwait(false);
            }
        }

        private async void MarkMessagesAsRead()
        {
            if (Messages.Count == 0)
            {
                return;
            }
            var lastMessage = Messages.SelectMany(x => x).Last();
            if (!lastMessage.IsRead)
            {
                await _chatManager.MarkMessageAsReadAsync(lastMessage.Id, _chatSummaryViewModel).ConfigureAwait(false);
            }
        }

        private async void DeleteMessage(ChatMessageViewModel message)
        {
            if (message == null)
            {
                return;
            }
            await _chatManager.DeleteMessageAsync(_chatSummaryViewModel.ChatId, message.Id).ConfigureAwait(false);
        }

        private void EditMessage(ChatMessageViewModel message)
        {
            SetMessageEditMode(true, message);
        }

        private void CancelEditingMessageMode()
        {
            SetMessageEditMode(false);
        }

        private void SetMessageEditMode(bool value, ChatMessageViewModel editedMessage = null)
        {
            if (value && editedMessage == null)
            {
                return;
            }

            _messageBeingEdited = value ? editedMessage : null;

            Execute.BeginOnUIThread(() =>
            {
                MessageToSendBody = editedMessage?.Body;
                IsInEditMessageMode = value;
            });
        }
    }
}
