// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Interfaces;

namespace Softeq.XToolkit.Chat
{
    public class ChatService : IChatService
    {
        private readonly ISocketChatAdapter _socketChatAdapter;
        private readonly IHttpChatAdapter _httpChatAdapter;
        private readonly ILogger _logger;

        private readonly Subject<ChatMessageModel> _messageReceived = new Subject<ChatMessageModel>();
        private readonly Subject<ChatMessageModel> _messageEdited = new Subject<ChatMessageModel>();

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private string _cachedUserId;

        public ChatService(
            ISocketChatAdapter socketChatAdapter,
            IHttpChatAdapter httpChatAdapter,
            ILogManager logManager)
        {
            _socketChatAdapter = socketChatAdapter;
            _httpChatAdapter = httpChatAdapter;
            _logger = logManager.GetLogger<ChatService>();

            _socketChatAdapter.MessageReceived.Subscribe(OnMessageReceived);
            _socketChatAdapter.MessageEdited.Subscribe(OnMessageEdited);
        }

        public IObservable<ChatMessageModel> MessageReceived => _messageReceived;
        public IObservable<ChatMessageModel> MessageEdited => _messageEdited;
        public IObservable<(string DeletedMessageId, ChatSummaryModel UpdatedChatSummary)> MessageDeleted => _socketChatAdapter.MessageDeleted;
        public IObservable<string> MessageRead => _socketChatAdapter.MessageRead;
        public IObservable<ChatSummaryModel> ChatAdded => _socketChatAdapter.ChatAdded;
        public IObservable<(string ChatId, bool IsMuted)> IsChatMutedChanged => _socketChatAdapter.IsChatMutedChanged;
        public IObservable<(string ChatId, int NewCount)> UnreadMessageCountChanged => _socketChatAdapter.UnreadMessageCountChanged;
        public IObservable<string> ChatRemoved => _socketChatAdapter.ChatRemoved;
        public IObservable<SocketConnectionStatus> ConnectionStatusChanged => _socketChatAdapter.ConnectionStatusChanged;

        public SocketConnectionStatus ConnectionStatus => _socketChatAdapter.ConnectionStatus;

        public async Task<ChatSummaryModel> CreateChatAsync(string chatName, IList<string> participantsIds)
        {
            var result = await _socketChatAdapter.CreateChatAsync(chatName, participantsIds);
            var userId = await GetUserIdAsync().ConfigureAwait(false);
            result.UpdateIsCreatedByMeStatus(userId);
            return result;
        }

        public Task CloseChatAsync(string chatId)
        {
            return _socketChatAdapter.CloseChatAsync(chatId);
        }

        public Task LeaveChatAsync(string chatId)
        {
            return _socketChatAdapter.LeaveChatAsync(chatId);
        }

        public Task InviteMembersAsync(string chatId, IList<string> participantsIds)
        {
            return _socketChatAdapter.InviteMembersAsync(chatId, participantsIds);
        }

        public virtual async Task<IList<ChatSummaryModel>> GetChatsHeadersAsync()
        {
            var chats = await _httpChatAdapter.GetChatsHeadersAsync().ConfigureAwait(false);
            if (chats != null)
            {
                var userId = await GetUserIdAsync().ConfigureAwait(false);
                chats.Apply(x => x.UpdateIsCreatedByMeStatus(userId));
                return chats.ToList();
            }
            return null;
        }

        public virtual async Task<IList<ChatMessageModel>> GetOlderMessagesAsync(string chatId,
                                                                         string messageFromId = null,
                                                                         DateTimeOffset? messageFromDateTime = null,
                                                                         int? count = null)
        {
            var messages = await _httpChatAdapter.GetOlderMessagesAsync(chatId, messageFromId, messageFromDateTime, count)
                                                 .ConfigureAwait(false);
            return await GetMessagesWithIsMineStatus(messages);
        }

        public virtual async Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId)
        {
            var messages = await _httpChatAdapter.GetLatestMessagesAsync(chatId).ConfigureAwait(false);
            return await GetMessagesWithIsMineStatus(messages);
        }

        public async Task<IList<ChatMessageModel>> GetMessagesFromAsync(string chatId,
                                                                        string messageFromId,
                                                                        DateTimeOffset messageFromDateTime,
                                                                        int? count = null)
        {
            var messages = await _httpChatAdapter.GetMessagesFromAsync(chatId, messageFromId, messageFromDateTime, count)
                                                 .ConfigureAwait(false);
            return await GetMessagesWithIsMineStatus(messages);
        }

        public async Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId)
        {
            var messages = await _httpChatAdapter.GetAllMessagesAsync(chatId).ConfigureAwait(false);
            return await GetMessagesWithIsMineStatus(messages);
        }

        public Task MarkMessageAsReadAsync(string chatId, string messageId)
        {
            return _httpChatAdapter.MarkMessageAsReadAsync(chatId, messageId);
        }

        public async Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody)
        {
            var result = await _socketChatAdapter.SendMessageAsync(chatId, messageBody);
            var userId = await GetUserIdAsync().ConfigureAwait(false);
            result?.UpdateIsMineStatus(userId);
            return result;
        }

        public Task EditMessageAsync(string messageId, string messageBody)
        {
            return _socketChatAdapter.EditMessageAsync(messageId, messageBody);
        }

        public Task DeleteMessageAsync(string chatId, string messageId)
        {
            return _socketChatAdapter.DeleteMessageAsync(chatId, messageId);
        }

        public Task<IList<ChatUserModel>> GetContactsAsync()
        {
            return _httpChatAdapter.GetContactsAsync();
        }

        public Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId)
        {
            return _httpChatAdapter.GetChatMembersAsync(chatId);
        }

        public void ForceReconnect()
        {
            _socketChatAdapter.ForceReconnect();
        }

        public void ForceDisconnect()
        {
            _socketChatAdapter.ForceDisconnect();
        }

        public void Logout()
        {
            ForceDisconnect();

            _cachedUserId = string.Empty;
        }

        private async Task<string> GetUserIdAsync()
        {
            var result = _cachedUserId;
            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(_cachedUserId))
                {
                    var userSummary = default(ChatUserModel);
                    do
                    {
                        userSummary = await _httpChatAdapter.GetUserSummaryAsync().ConfigureAwait(false);
                    } while (string.IsNullOrEmpty(userSummary?.SaasUserId));
                    _cachedUserId = userSummary.SaasUserId;
                }
                result = _cachedUserId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            return result;
        }

        private async void OnMessageReceived(ChatMessageModel messageModel)
        {
            var userId = await GetUserIdAsync().ConfigureAwait(false);
            messageModel.UpdateIsMineStatus(userId);
            _messageReceived.OnNext(messageModel);
        }

        private async void OnMessageEdited(ChatMessageModel messageModel)
        {
            var userId = await GetUserIdAsync().ConfigureAwait(false);
            messageModel.UpdateIsMineStatus(userId);
            _messageEdited.OnNext(messageModel);
        }

        private async Task<IList<ChatMessageModel>> GetMessagesWithIsMineStatus(IList<ChatMessageModel> messages)
        {
            if (messages != null)
            {
                var userId = await GetUserIdAsync().ConfigureAwait(false);
                messages.Apply(x => x.UpdateIsMineStatus(userId));
                return messages.ToList();
            }
            return null;
        }
    }
}
