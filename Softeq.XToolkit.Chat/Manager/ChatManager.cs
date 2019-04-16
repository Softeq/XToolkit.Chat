// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Messenger;

namespace Softeq.XToolkit.Chat.Manager
{
    public partial class ChatManager : IChatConnectionManager
    {
        private const string ChatsCacheKey = "chat_chats";

        private readonly IChatService _chatService;
        private readonly IMessagesCache _messagesCache;
        private readonly IViewModelFactoryService _viewModelFactoryService;
        private readonly IUploadImageService _uploadImageService;
        private readonly ILocalCache _localCache;
        private readonly ILogger _logger;

        private readonly IList<IDisposable> _subscriptions = new List<IDisposable>();

        private readonly ISubject<ChatMessageViewModel> _messageAdded = new Subject<ChatMessageViewModel>();
        private readonly ISubject<ChatMessageModel> _messageEdited = new Subject<ChatMessageModel>();
        private readonly ISubject<string> _messageDeleted = new Subject<string>();
        private readonly ISubject<IList<ChatMessageViewModel>> _messagesBatchAdded = new Subject<IList<ChatMessageViewModel>>();
        private readonly ISubject<IList<ChatMessageModel>> _messagesBatchUpdated = new Subject<IList<ChatMessageModel>>();
        private readonly ISubject<IList<string>> _messagesBatchDeleted = new Subject<IList<string>>();
        private readonly ISubject<ConnectionStatus> _connectionStatusChanged = new Subject<ConnectionStatus>();

        private ConnectionStatus _connectionStatus = ConnectionStatus.Online;
        private string _activeChatId;

        public ChatManager(
            IChatService chatService,
            IMessagesCache messagesCache,
            IViewModelFactoryService viewModelFactoryService,
            ILogManager logManager,
            IUploadImageService uploadImageService,
            ILocalCache localCache)
        {
            _chatService = chatService;
            _messagesCache = messagesCache;
            _viewModelFactoryService = viewModelFactoryService;
            _uploadImageService = uploadImageService;
            _localCache = localCache;
            _logger = logManager.GetLogger<ChatManager>();

            _messagesCache.Init(new Common.TaskReference<string, string, DateTimeOffset, IList<ChatMessageModel>>(
                (chatId, messageFromId, messageFromDateTime) =>
            {
                return _chatService.GetMessagesFromAsync(chatId, messageFromId, messageFromDateTime);
            }));
            _messagesCache.CacheUpdated += OnCacheUpdated;

            _subscriptions.Add(_chatService.MessageReceived.Subscribe(AddLatestMessage));
            _subscriptions.Add(_chatService.MessageEdited.Subscribe(OnMessageEdited));
            _subscriptions.Add(_chatService.MessageDeleted.Subscribe(OnMessageDeleted));
            _subscriptions.Add(_chatService.ChatAdded.Subscribe(x => TryAddChat(x)));
            _subscriptions.Add(_chatService.ChatRemoved.Subscribe(OnChatRemoved));

            _subscriptions.Add(_chatService.ConnectionStatusChanged.Subscribe(OnConnectionStatusChanged));
            OnConnectionStatusChanged(_chatService.ConnectionStatus);

            Messenger.Default.Register<ChatInForegroundMessage>(this, x => _chatService.ForceReconnect());
            Messenger.Default.Register<ChatInBackgroundMessage>(this, x => _chatService.ForceDisconnect());
        }

        public ConnectionStatus ConnectionStatus
        {
            get => _connectionStatus;
            private set
            {
                _connectionStatus = value;
                _connectionStatusChanged?.OnNext(_connectionStatus);
            }
        }

        public IObservable<ChatMessageViewModel> MessageAdded => _messageAdded;
        public IObservable<ChatMessageModel> MessageEdited => _messageEdited;
        public IObservable<string> MessageDeleted => _messageDeleted;
        public IObservable<IList<ChatMessageViewModel>> MessagesBatchAdded => _messagesBatchAdded;
        public IObservable<IList<ChatMessageModel>> MessagesBatchUpdated => _messagesBatchUpdated;
        public IObservable<IList<string>> MessagesBatchDeleted => _messagesBatchDeleted;
        public IObservable<ConnectionStatus> ConnectionStatusChanged => _connectionStatusChanged;
        public IObservable<string> ChatRead => _chatService.ChatRead;

        public ObservableRangeCollection<ChatSummaryViewModel> ChatsCollection { get; }
            = new ObservableRangeCollection<ChatSummaryViewModel>();

        public void Logout()
        {
            _chatService.Logout();

            _messagesCache.FullCleanUp();

            ChatsCollection.Clear();
        }

        private void OnConnectionStatusChanged(SocketConnectionStatus connectionStatus)
        {
            switch (connectionStatus)
            {
                case SocketConnectionStatus.Connected:
                    UpdateCacheAsync().SafeTaskWrapper();
                    break;
                case SocketConnectionStatus.Connecting:
                    ConnectionStatus = ConnectionStatus.Connecting;
                    break;
                case SocketConnectionStatus.WaitingForNetwork:
                    ConnectionStatus = ConnectionStatus.WaitingForNetwork;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        private async Task UpdateCacheAsync()
        {
            ConnectionStatus = ConnectionStatus.Updating;

            await UpdateChatsListFromNetworkAsync().ConfigureAwait(false);
            await UpdateMessagesCacheAsync().ConfigureAwait(false);

            ConnectionStatus = ConnectionStatus.Online;
        }

        private void OnCacheUpdated(CacheUpdatedResults cacheUpdatedResults)
        {
            if (cacheUpdatedResults.ChatId != _activeChatId)
            {
                return;
            }

            var addedMessagesViewModels = CreateMessagesViewModels(cacheUpdatedResults.NewMessages);

            if (addedMessagesViewModels.Any())
            {
                _messagesBatchAdded.OnNext(addedMessagesViewModels);
            }

            if (cacheUpdatedResults.UpdatedMessages.Any())
            {
                _messagesBatchUpdated.OnNext(cacheUpdatedResults.UpdatedMessages);
            }

            if (cacheUpdatedResults.DeletedMessagesIds.Any())
            {
                _messagesBatchDeleted.OnNext(cacheUpdatedResults.DeletedMessagesIds);
            }
        }
    }
}
