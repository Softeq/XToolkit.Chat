// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Common.Models;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IHttpChatAdapter
    {
        Task<ChatUserModel> GetUserSummaryAsync();

        Task<IList<ChatSummaryModel>> GetChannelsAsync();

        Task<IList<ChatMessageModel>> GetOlderMessagesAsync(string chatId,
            string messageFromId = null, DateTimeOffset? messageFromDateTime = null, int? count = null);

        Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId);

        Task<IList<ChatMessageModel>> GetMessagesFromAsync(string chatId,
            string messageFromId, DateTimeOffset messageFromDateTime, int? count = null);

        Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId);

        Task MarkMessageAsReadAsync(string chatId, string messageId);

        Task MuteChatAsync(string chatId);

        Task UnMuteChatAsync(string chatId);

        Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId);

        Task<PagingModel<ChatUserModel>> GetContactsAsync(string nameFilter, int pageSize, int pageNumber);

        Task<PagingModel<ChatUserModel>> GetContactsForInviteAsync(string chatId,
            string nameFilter, int pageSize, int pageNumber);
    }
}
