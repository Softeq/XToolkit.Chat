// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.EventArguments;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Interfaces;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatMessagesListViewModel : ObservableObject
    {
        private const int InitialReadMessagesBatchCount = 20;
        private const int OlderMessagesBatchCount = 50;

        private readonly string _chatId;
        private readonly IChatMessagesManager _chatManager;
        private readonly Action _messageAdded;
        private readonly IList<IDisposable> _subscriptions = new List<IDisposable>();

        private bool _areLatestMessagesLoaded;
        private bool _areOlderMessagesLoaded;

        public ChatMessagesListViewModel(
            string chatId,
            IChatMessagesManager chatManager,
            Action messageAdded)
        {
            _chatId = chatId;
            _chatManager = chatManager;
            _messageAdded = messageAdded;

            LoadOlderMessagesCommand = new RelayCommand(() => LoadOlderMessagesAsync().SafeTaskWrapper());
        }

        public ICommand LoadOlderMessagesCommand { get; }

        public ObservableKeyGroupsCollection<DateTimeOffset, ChatMessageViewModel> Messages { get; }
            = new ObservableKeyGroupsCollection<DateTimeOffset, ChatMessageViewModel>(message => message.DateTime.Date,
                (x, y) => x.CompareTo(y),
                (x, y) => x.DateTime.CompareTo(y.DateTime));

        public void OnAppearing()
        {
            Subscribe(_chatManager.MessageAdded, OnMessageReceived);
            Subscribe(_chatManager.MessageEdited, OnMessageEdited);
            Subscribe(_chatManager.MessageDeleted, OnMessageDeleted);
            Subscribe(_chatManager.MessagesBatchAdded, OnMessagesBatchReceived);
            Subscribe(_chatManager.MessagesBatchUpdated, OnMessagesBatchUpdated);
            Subscribe(_chatManager.MessagesBatchDeleted, OnMessagesBatchDeleted);
            Subscribe(_chatManager.ChatRead, OnChatRead);

            if (!_areLatestMessagesLoaded)
            {
                LoadInitialMessagesAsync().SafeTaskWrapper();
                _areLatestMessagesLoaded = true;
            }

            Messages.ItemsChanged += OnMessagesAddedToCollection;
            MarkMessagesAsRead();
        }

        public void OnDisappearing()
        {
            RemoveAllSubscriptions();

            Messages.ItemsChanged -= OnMessagesAddedToCollection;
        }

        public void OnMessageDeleted(string deletedMessageId)
        {
            DeleteAllMessages(x => x.Id == deletedMessageId);
        }

        public void OnMessageReceived(ChatMessageViewModel messageViewModel)
        {
            if (messageViewModel.ChatId == _chatId)
            {
                AddNewMessages(new List<ChatMessageViewModel> { messageViewModel });
            }
        }

        public void OnMessagesBatchReceived(IList<ChatMessageViewModel> messages)
        {
            var messagesToAdd = messages.Where(x => x.ChatId == _chatId).ToList();
            AddNewMessages(messagesToAdd);
        }

        public void OnMessageEdited(ChatMessageModel messageModel)
        {
            bool WasUpdated(ChatMessageViewModel x) => x.Id == messageModel.Id;
            Messages.Where(x => x.Any(WasUpdated))
                    .SelectMany(x => x)
                    .Where(WasUpdated)
                    .Apply(x => x.UpdateMessageModel(messageModel));
        }

        public void OnMessagesBatchUpdated(IList<ChatMessageModel> messagesModels)
        {
            foreach (var m in messagesModels)
            {
                OnMessageEdited(m);
            }
        }

        public void OnMessagesBatchDeleted(IList<string> deletedMessagesIds)
        {
            DeleteAllMessages(x => deletedMessagesIds.Contains(x.Id));
        }

        public void OnChatRead(string chatId)
        {
            if (chatId != _chatId)
            {
                return;
            }

            Messages.SelectMany(x => x)
                    .Where(x => x.IsMine)
                    .Apply(x => x.MarkAsRead());
        }

        private void AddNewMessages(IList<ChatMessageViewModel> messages)
        {
            Messages.AddRangeToGroupsSorted(messages);

            _messageAdded.Invoke();

            MarkMessagesAsRead();
        }

        // TODO YP: check frequency of call this method
        public async Task LoadOlderMessagesAsync()
        {
            if (_areOlderMessagesLoaded)
            {
                return;
            }

            var oldestMessage = Messages.FirstOrDefaultValue();
            if (oldestMessage == null)
            {
                await LoadInitialMessagesAsync();
                return;
            }
            var olderMessages = await _chatManager.LoadOlderMessagesAsync(
                _chatId,
                oldestMessage.Id,
                oldestMessage.DateTime,
                OlderMessagesBatchCount);

            if (olderMessages.Any())
            {
                Messages.AddRangeToGroupsSorted(olderMessages);
            }
            else
            {
                // empty list = no old messages
                _areOlderMessagesLoaded = true;
            }
        }

        private async Task LoadInitialMessagesAsync()
        {
            var messages = await _chatManager.LoadInitialMessagesAsync(_chatId, InitialReadMessagesBatchCount);
            Messages.AddRangeToGroupsSorted(messages);
        }

        private void OnMessagesAddedToCollection(object sender, NotifyKeyGroupsCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                MarkMessagesAsRead();
            }
        }

        private async void MarkMessagesAsRead()
        {
            if (Messages.Count == 0)
            {
                return;
            }
            var lastMessage = Messages.SelectMany(x => x).Last();
            if (!lastMessage.IsRead && !lastMessage.IsMine)
            {
                await _chatManager.MarkMessageAsReadAsync(_chatId, lastMessage.Id).ConfigureAwait(false);
            }
        }

        // TODO YP: unused
        //private void ClearMessages()
        //{
        //    _areLatestMessagesLoaded = false;
        //    Messages.ItemsChanged -= OnMessagesAddedToCollection;
        //    Messages.ClearAll();
        //}

        private void DeleteAllMessages(Func<ChatMessageViewModel, bool> predicate)
        {
            var messagesToDelete = Messages
                .Where(x => x.Any(predicate))
                .SelectMany(x => x)
                .Where(predicate)
                .ToList();
            Messages.RemoveAllFromGroups(messagesToDelete);
        }

        private void Subscribe<T>(IObservable<T> observer, Action<T> handler)
        {
            _subscriptions.Add(observer.Subscribe(handler));
        }

        private void RemoveAllSubscriptions()
        {
            _subscriptions.Apply(x => x.Dispose());
        }
    }
}
