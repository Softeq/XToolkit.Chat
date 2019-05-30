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
using Softeq.XToolkit.Common.Models;

namespace Softeq.XToolkit.Chat
{
    public class ChatService : IChatService
    {
        private readonly ISocketChatAdapter _socketChatAdapter;
        private readonly IHttpChatAdapter _httpChatAdapter;
        private readonly ILogger _logger;

        private readonly ISubject<ChatMessageModel> _messageReceived = new Subject<ChatMessageModel>();
        private readonly ISubject<ChatMessageModel> _messageEdited = new Subject<ChatMessageModel>();

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
        public IObservable<string> ChatRead => _socketChatAdapter.ChatRead;
        public IObservable<SocketConnectionStatus> ConnectionStatusChanged => _socketChatAdapter.ConnectionStatusChanged;

        public SocketConnectionStatus ConnectionStatus => _socketChatAdapter.ConnectionStatus;

        public async Task<ChatSummaryModel> CreateChatAsync(string chatName, IList<string> participantsIds, string imagePath)
        {
            var result = await _socketChatAdapter.CreateChatAsync(chatName, participantsIds, imagePath);
            if (result == null)
            {
                return null;
            }
            var userId = await GetUserIdAsync().ConfigureAwait(false);
            result.UpdateIsCreatedByMeStatus(userId);
            return result;
        }

        public async Task<ChatSummaryModel> CreateDirectChatAsync(string memberId)
        {
            var result = await _socketChatAdapter.CreateDirectChatAsync(memberId).ConfigureAwait(false);

            if (result == null)
            {
                return null;
            }

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

        public Task DeleteMemberAsync(string chatId, string memberId)
        {
            return _socketChatAdapter.DeleteMemberAsync(chatId, memberId);
        }

        public Task MuteChatAsync(string chatId)
        {
            return _httpChatAdapter.MuteChatAsync(chatId);
        }

        public Task UnMuteChatAsync(string chatId)
        {
            return _httpChatAdapter.UnMuteChatAsync(chatId);
        }

        public virtual async Task<IList<ChatSummaryModel>> GetChatsListAsync()
        {
            var models = await _httpChatAdapter.GetChannelsAsync().ConfigureAwait(false);
            if (models == null)
            {
                return null;
            }

            var userId = await GetUserIdAsync().ConfigureAwait(false);
            models.Apply(x => x.UpdateIsCreatedByMeStatus(userId));

            return models.ToList();
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

        public async Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody, string imageUrl)
        {
            var result = await _socketChatAdapter.SendMessageAsync(chatId, messageBody, imageUrl);
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

        public Task<PagingModel<ChatUserModel>> GetContactsAsync(string nameFilter, int pageNumber, int pageSize)
        {
            return _httpChatAdapter.GetContactsAsync(nameFilter, pageNumber, pageSize);
        }

        public Task<PagingModel<ChatUserModel>> GetContactsForInviteAsync(string chatId, string nameFilter, int pageNumber, int pageSize)
        {
            return _httpChatAdapter.GetContactsForInviteAsync(chatId, nameFilter, pageNumber, pageSize);
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

        public Task EditChatAsync(ChatSummaryModel chatSummary)
        {
            return _socketChatAdapter.EditChatAsync(chatSummary);
        }

        public void Logout()
        {
            ForceDisconnect();

            _cachedUserId = string.Empty;
        }

        // TODO YP: move to backend side
        private async Task<string> GetUserIdAsync()
        {
            var result = _cachedUserId;
            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(_cachedUserId))
                {
                    ChatUserModel userSummary;
                    do
                    {
                        userSummary = await _httpChatAdapter.GetUserSummaryAsync().ConfigureAwait(false);
                    }
                    while (string.IsNullOrEmpty(userSummary?.Id));
                    _cachedUserId = userSummary.Id;
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

        private Task<ChatUserModel> GetUserSummaryAsync()
        {
            return _httpChatAdapter.GetUserSummaryAsync();
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
