// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models.Queries;
using Softeq.XToolkit.Common.Tasks;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IMessagesCache
    {
        void Init(TaskReference<MessagesQuery, IList<ChatMessageModel>> getMessagesAsync);

        event Action<CacheUpdatedResults> CacheUpdated;

        Task<List<ChatMessageModel>> GetLatestMessagesAsync(string chatId, int count);
        Task<List<ChatMessageModel>> GetOlderMessagesAsync(MessagesQuery query);

        void TryAddMessage(ChatMessageModel chatMessage);
        void TryEditMessage(ChatMessageModel updatedMessage);
        void TryDeleteMessage(string channelId, string deletedMessageId);
        void UpdateSentMessage(ChatMessageModel sentMessage, ChatMessageModel deliveredMessage);

        ChatMessageModel FindDuplicateMessage(ChatMessageModel message);

        Task SaveMessagesAsync(string chatId, IList<ChatMessageModel> messages);
        Task RemoveMessagesAsync(string chatId);
        void ReadMyLatestMessages(string chatId);

        Task PerformFullUpdate(IList<string> chatIds);

        void FullCleanUp();
    }
}
