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
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Interfaces;

namespace Softeq.XToolkit.Chat
{
    public class InMemoryMessagesCache : IMessagesCache
    {
        private readonly ILogger _logger;

        private readonly ConcurrentDictionary<string, List<ChatMessageModel>> _messages
            = new ConcurrentDictionary<string, List<ChatMessageModel>>();

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private TaskReference<string, string, DateTimeOffset, IList<ChatMessageModel>> _getMessagesAsync;

        public event Action<string, IList<ChatMessageModel>, IList<ChatMessageModel>, IList<string>> CacheUpdated;

        public InMemoryMessagesCache(ILogManager logManager)
        {
            _logger = logManager.GetLogger<ChatService>();
        }

        public void Init(TaskReference<string, string, DateTimeOffset, IList<ChatMessageModel>> getMessagesAsync)
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
                    return messagesCollection;
                }
                var result = new List<ChatMessageModel>();
                var indexFrom = Math.Max(0, lastReadMessageIndex - count + 1);
                for (int i = indexFrom; i < collection.Count; i++)
                {
                    result.Add(collection[i]);
                }
                return result;
            }));
        }

        public Task<List<ChatMessageModel>> GetOlderMessagesAsync(string chatId, string messageFromId, DateTimeOffset messageFromDateTime, int count)
        {
            var messagesCollection = GetMessagesCollectionForChat(chatId);
            return Task.FromResult(ModifyCollection(messagesCollection, collection =>
            {
                var indexFrom = collection.FindIndex(x => x.Id == messageFromId);
                if (indexFrom < 0)
                {
                    indexFrom = collection.FindLastIndex(x => x.DateTime.DateTime.IsEarlierOrEqualThan(messageFromDateTime.DateTime));
                }
                if (indexFrom > count)
                {
                    var result = new List<ChatMessageModel>();
                    for (int i = indexFrom - 1; i >= indexFrom - count; i--)
                    {
                        result.Insert(0, collection[i]);
                    }
                    return result;
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
                x.Where(y => y.Equals(updatedMessage)).Apply(y => y.UpdateMessage(updatedMessage));
            });
        }

        public void TryDeleteMessage(string chatId, string deletedMessageId)
        {
            var collection = GetMessagesCollectionForChat(chatId);
            ModifyCollection(collection, x => x.RemoveAll(y => y.Id == deletedMessageId));
        }

        public ChatMessageModel FindDuplicateMessage(ChatMessageModel message)
        {
            var chatMessages = GetMessagesCollectionForChat(message.ChannelId);
            return chatMessages.FirstOrDefault(x => (x.Id == null || x.Id == message.Id) &&
                                               x.Body == message.Body &&
                                               x.IsMine == message.IsMine);
        }

        public void UpdateSentMessage(ChatMessageModel sentMessage, ChatMessageModel deliveredMessage)
        {
            var collection = GetMessagesCollectionForChat(sentMessage.ChannelId);
            ModifyCollection(collection, x =>
            {
                var messagesToUpdate = x.Where(y => y.Equals(sentMessage)).ToList();
                if (messagesToUpdate.Count != 1)
                {
                    throw new ChatCacheException("There must be only one message to update after sending");
                }
                messagesToUpdate.Apply(y => y.UpdateMessage(deliveredMessage));
            });
        }

        public Task SaveMessagesAsync(string chatId, IList<ChatMessageModel> messages)
        {
            var collection = GetMessagesCollectionForChat(chatId);
            ModifyCollection(collection, x => AddNewMessages(x, messages));
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

        public async Task PerformFullUpdate(IList<string> chatIds)
        {
            if (_getMessagesAsync == null)
            {
                throw new ChatCacheException("You must call Init method first. GetMessages delegate must not be null");
            }
            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
                var tasks = chatIds.Where(x => x != null && _messages.ContainsKey(x)).Select(UpdateChatMessages).ToList();
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
            var messagesCollection = GetMessagesCollectionForChat(chatId);

            var oldestMessage = default(ChatMessageModel);
            var newestMessage = default(ChatMessageModel);
            ModifyCollection(messagesCollection, collection =>
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

            var upToDateMessages = default(IList<ChatMessageModel>);
            try
            {
                upToDateMessages = await _getMessagesAsync.RunAsync(chatId, oldestMessage.Id, oldestMessage.DateTime).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            if (upToDateMessages == null)
            {
                await UpdateChatMessages(chatId);
                return;
            }

            ModifyCollection(messagesCollection, collection =>
            {
                var messagesToDelete = collection
                    .Where(x => x.MessageType != MessageType.Info)
                    .Except(upToDateMessages)
                    .SkipWhile(x => x.IsEarlierThan(oldestMessage))
                    .TakeWhile(x => x.Status != ChatMessageStatus.Sending && x.IsEarlierOrEqualsThan(newestMessage))
                    .ToList();
                messagesToDelete.AddRange(collection.Where(x => x == null));
                collection.RemoveAll(x => messagesToDelete.Contains(x));

                var updatedMessages = GetUpdatedMessagesWithUpdateCollection(collection, upToDateMessages);

                var newMessages = upToDateMessages.Except(collection).Where(x => x != null).ToList();
                AddNewMessages(collection, newMessages);

                NotifyCacheUpdated(chatId, newMessages, updatedMessages, messagesToDelete.Select(x => x.Id).ToList());
            });
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
                    currentMessage.ImageUrl != newMessage.ImageUrl ||
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
            var messagesToInsert = messages.Where(y => !collection.Contains(y)).ToList();
            foreach (var message in messages)
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

        private void NotifyCacheUpdated(
            string chatId,
            IList<ChatMessageModel> addedMessages,
            IList<ChatMessageModel> updatedMessages,
            IList<string> deletedMessagesIds)
        {
            CacheUpdated?.Invoke(chatId, addedMessages, updatedMessages, deletedMessagesIds);
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
    }
}
