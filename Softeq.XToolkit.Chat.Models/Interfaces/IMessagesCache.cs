// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Common;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IMessagesCache
    {
        void Init(TaskReference<string, string, DateTimeOffset, IList<ChatMessageModel>> getMessagesAsync);

        event Action<string, IList<ChatMessageModel>, IList<ChatMessageModel>, IList<string>> CacheUpdated;

        Task<List<ChatMessageModel>> GetLatestMessagesAsync(string chatId, int count);
        Task<List<ChatMessageModel>> GetOlderMessagesAsync(string chatId, string messageFromId, DateTimeOffset messageFromDateTime, int count);

        void TryAddMessage(ChatMessageModel chatMessage);
        void TryEditMessage(ChatMessageModel updatedMessage);
        void TryDeleteMessage(string chatId, string deletedMessageId);
        void UpdateSentMessage(ChatMessageModel sentMessage, ChatMessageModel deliveredMessage);

        ChatMessageModel FindDuplicateMessage(ChatMessageModel message);

        Task SaveMessagesAsync(string chatId, IList<ChatMessageModel> messages);
        Task RemoveMessagesAsync(string chatId);

        Task PerformFullUpdate(IList<string> chatIds);
    }
}
