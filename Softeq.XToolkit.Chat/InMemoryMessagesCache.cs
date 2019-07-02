// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Exceptions;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.Models.Queries;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Interfaces;

namespace Softeq.XToolkit.Chat
{
    // TODO YP: hard refactoring needed
    public class InMemoryMessagesCache : IMessagesCache
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, List<ChatMessageModel>> _messages
            = new ConcurrentDictionary<string, List<ChatMessageModel>>();

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private TaskReference<MessagesQuery, IList<ChatMessageModel>> _getMessagesAsync;

        public event Action<CacheUpdatedResults> CacheUpdated;

        public InMemoryMessagesCache(ILogManager logManager)
        {
            _logger = logManager.GetLogger<ChatService>();
        }

        public void Init(TaskReference<MessagesQuery, IList<ChatMessageModel>> getMessagesAsync)
        {
            _getMessagesAsync = getMessagesAsync;
        }

        public Task<List<ChatMessageModel>> GetLatestMessagesAsync(string chatId, int count = 20)
        {
            var messagesCollection = GetMessagesCollectionForChat(chatId);
            if (messagesCollection.Count == 0)
            {
                return Task.FromResult(new List<ChatMessageModel>());
            }
            return Task.FromResult(ModifyCollection(messagesCollection, collection =>
            {
                var lastReadMessageIndex = collection.FindLastIndex(x => x.IsRead);
                if (lastReadMessageIndex < 0)
                {
                    return collection; // all unread messages
                }

                return collection.Skip(lastReadMessageIndex - count + 1).ToList();
            }));
        }

        public Task<List<ChatMessageModel>> GetOlderMessagesAsync(MessagesQuery query)
        {
            var messagesCollection = GetMessagesCollectionForChat(query.ChannelId);
            return Task.FromResult(ModifyCollection(messagesCollection, collection =>
            {
                // find older message than
                var indexFrom = collection.FindIndex(x => x.Id == query.FromId);
                if (indexFrom < 0)
                {
                    indexFrom = collection.FindLastIndex(x =>
                        x.DateTime.DateTime.IsEarlierOrEqualThan(query.FromDateTime.Value.DateTime));
                }

                var count = query.Count.Value;
                if (indexFrom > count)
                {
                    return collection.Skip(indexFrom - count).Take(count).ToList();
                }

                if (indexFrom > 0)
                {
                    return collection.Take(indexFrom).ToList();
                }
                return new List<ChatMessageModel>();
            }));
        }

        public void TryAddMessage(ChatMessageModel chatMessage)
        {
            var collection = GetMessagesCollectionForChat(chatMessage.ChannelId);
            ModifyCollection(collection, x => x.Add(chatMessage));
        }

        public void TryEditMessage(ChatMessageModel updatedMessage)
        {
            var collection = GetMessagesCollectionForChat(updatedMessage.ChannelId);
            ModifyCollection(collection, x =>
            {
                x.Where(y => y.Equals(updatedMessage))
                 .Apply(y => y.UpdateMessage(updatedMessage));
            });
        }

        public void TryDeleteMessage(string channelId, string deletedMessageId)
        {
            var channelMessages = GetMessagesCollectionForChat(channelId);
            ModifyCollection(channelMessages, x => x.RemoveAll(y => y.Id == deletedMessageId));
        }

        public ChatMessageModel FindDuplicateMessage(ChatMessageModel message)
        {
            var messages = GetMessagesCollectionForChat(message.ChannelId);
            ChatMessageModel duplicatedMessage = null;

            lock (messages)
            {
                duplicatedMessage = messages.FirstOrDefault(x =>
                    (x.Id == null || x.Id == message.Id) &&
                    x.Body == message.Body &&
                    x.IsMine == message.IsMine);
            }

            return duplicatedMessage;
        }

        public void UpdateSentMessage(ChatMessageModel sentMessage, ChatMessageModel deliveredMessage)
        {
            var messages = GetMessagesCollectionForChat(sentMessage.ChannelId);
            ModifyCollection(messages, x =>
            {
                var messagesToUpdate = x.Where(y => y.Equals(sentMessage)).ToList();
                if (messagesToUpdate.Count != 1)
                {
                    throw new ChatCacheException("There must be only one message to update after sending");
                }
                messagesToUpdate.Apply(y => y.UpdateMessage(deliveredMessage));
            });
        }

        public Task SaveMessagesAsync(string chatId, IList<ChatMessageModel> newMessages)
        {
            var messages = GetMessagesCollectionForChat(chatId);
            ModifyCollection(messages, x => AddNewMessages(x, newMessages));
            return Task.FromResult(true);
        }

        public Task RemoveMessagesAsync(string chatId)
        {
            if (chatId == null)
            {
                return null;
            }
            if (_messages.ContainsKey(chatId))
            {
                _messages.TryRemove(chatId, out List<ChatMessageModel> _);
            }
            return Task.FromResult(true);
        }

        public void ReadMyLatestMessages(string chatId)
        {
            var messages = GetMessagesCollectionForChat(chatId);
            ModifyCollection(messages, x =>
            {
                x.Where(y => y.IsMine && !y.IsRead)
                 .Apply(y => y.ReadMessage());
            });
        }

        public async Task PerformFullUpdate(IList<string> chatIds)
        {
            if (_getMessagesAsync == null)
            {
                throw new ChatCacheException("You must call Init method first. GetMessages delegate must not be null");
            }
            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

                var tasks = chatIds
                    .Where(x => x != null && _messages.ContainsKey(x))
                    .Select(UpdateChatMessages)
                    .ToList();

                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task UpdateChatMessages(string chatId)
        {
            var messages = GetMessagesCollectionForChat(chatId);

            var oldestMessage = default(ChatMessageModel);
            var newestMessage = default(ChatMessageModel);
            ModifyCollection(messages, collection =>
            {
                oldestMessage = collection.FirstOrDefault();
                if (oldestMessage != null)
                {
                    newestMessage = collection.Last();
                }
            });
            if (oldestMessage == null)
            {
                return;
            }

            var upToDateMessages = await TryLoadLatestMessagesAsync(chatId,
                oldestMessage.Id, oldestMessage.DateTime).ConfigureAwait(false);

            if (upToDateMessages == null)
            {
                await UpdateChatMessages(chatId); // TODO YP: check recursion
                return;
            }

            ModifyCollection(messages, collection =>
            {
                // find messages for delete
                var messagesToDelete = collection
                    .Where(x => x.MessageType != MessageType.System)
                    .Except(upToDateMessages)
                    .SkipWhile(x => x.IsEarlierThan(oldestMessage))
                    .TakeWhile(x => x.Status != ChatMessageStatus.Sending && x.IsEarlierOrEqualsThan(newestMessage))
                    .ToList();
                messagesToDelete.AddRange(collection.Where(x => x == null));
                collection.RemoveAll(x => messagesToDelete.Contains(x));

                // find messages for update
                var updatedMessages = GetUpdatedMessagesWithUpdateCollection(collection, upToDateMessages);

                // find new messages
                var newMessages = upToDateMessages.Except(collection).Where(x => x != null).ToList();
                AddNewMessages(collection, newMessages);

                var updatedResults = new CacheUpdatedResults
                {
                    ChatId = chatId,
                    NewMessages = newMessages,
                    UpdatedMessages = updatedMessages,
                    DeletedMessagesIds = messagesToDelete.Select(x => x.Id).ToList()
                };

                CacheUpdated?.Invoke(updatedResults);
            });
        }

        private async Task<IList<ChatMessageModel>> TryLoadLatestMessagesAsync(
            string chatId,
            string oldestMessageId,
            DateTimeOffset oldestMessageDate)
        {
            var messages = default(IList<ChatMessageModel>);
            try
            {
                var query = new MessagesQuery
                {
                    ChannelId = chatId,
                    FromId = oldestMessageId,
                    FromDateTime = oldestMessageDate
                };
                messages = await _getMessagesAsync.RunAsync(query).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return messages;
        }

        private IList<ChatMessageModel> GetUpdatedMessagesWithUpdateCollection(
            IList<ChatMessageModel> collection,
            IList<ChatMessageModel> upToDateMessages)
        {
            var updatedMessages = new List<ChatMessageModel>();
            foreach (var message in collection)
            {
                var updatedMessage = upToDateMessages.FirstOrDefault(x => IsMessageChanged(message, x));
                if (updatedMessage != null)
                {
                    updatedMessages.Add(updatedMessage);

                    message.UpdateMessage(updatedMessage);
                }
            }
            return updatedMessages;
        }

        private bool IsMessageChanged(ChatMessageModel currentMessage, ChatMessageModel newMessage)
        {
            return currentMessage.Equals(newMessage) &&
                   (currentMessage.Status != newMessage.Status ||
                    currentMessage.ImageRemoteUrl != newMessage.ImageRemoteUrl ||
                    currentMessage.SenderName != newMessage.SenderName ||
                    currentMessage.SenderPhotoUrl != newMessage.SenderPhotoUrl ||
                    currentMessage.Body != newMessage.Body);
        }

        /// <summary>
        /// Use this method only inside ModifyCollection method
        /// </summary>
        /// <param name="collection">Common collection.</param>
        /// <param name="messages">Collection to insert.</param>
        private void AddNewMessages(List<ChatMessageModel> collection, IList<ChatMessageModel> messages)
        {
            var messagesToInsert = messages.Except(collection).ToList();
            foreach (var message in messagesToInsert)
            {
                var previousMessageIndex = collection.FindLastIndex(y => y.DateTime.DateTime.IsEarlierThan(message.DateTime.DateTime));
                collection.Insert(previousMessageIndex + 1, message);
            }
        }

        private List<ChatMessageModel> GetMessagesCollectionForChat(string chatId)
        {
            if (chatId == null)
            {
                return null;
            }
            if (!_messages.ContainsKey(chatId))
            {
                _messages.TryAdd(chatId, new List<ChatMessageModel>());
            }
            return _messages[chatId];
        }

        private static void ModifyCollection(List<ChatMessageModel> collection, Action<List<ChatMessageModel>> modify)
        {
            ModifyCollection<bool>(collection, x =>
            {
                modify(x);
                return true;
            });
        }

        private static T ModifyCollection<T>(List<ChatMessageModel> collection, Func<List<ChatMessageModel>, T> modify)
        {
            var result = default(T);
            lock (collection)
            {
                result = modify(collection);
            }
            return result;
        }

        public void FullCleanUp()
        {
            _messages.Clear();
        }
    }
}
