// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models.Queries;
using Softeq.XToolkit.Common.Models;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IHttpChatAdapter
    {
        // chats
        Task<IList<ChatSummaryModel>> GetChannelsAsync();
        Task<ChatSummaryModel> CreateDirectChatAsync(string memberId);
        Task MuteChatAsync(string chatId);
        Task UnMuteChatAsync(string chatId);

        // messages
        Task<IList<ChatMessageModel>> GetOlderMessagesAsync(MessagesQuery query);
        Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId);
        Task<IList<ChatMessageModel>> GetMessagesFromAsync(MessagesQuery query);
        Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId);
        Task MarkMessageAsReadAsync(string chatId, string messageId);

        // members
        Task<ChatUserModel> GetUserSummaryAsync();
        Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId);
        Task<PagingModel<ChatUserModel>> GetContactsAsync(string nameFilter, int pageSize, int pageNumber);
        Task<PagingModel<ChatUserModel>> GetContactsForInviteAsync(ContactsQuery query);
    }
}
