// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Common.Models;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IChatService
    {
        IObservable<ChatMessageModel> MessageReceived { get; }
        IObservable<(string DeletedMessageId, ChatSummaryModel UpdatedChatSummary)> MessageDeleted { get; }
        IObservable<string> MessageRead { get; }
        IObservable<ChatMessageModel> MessageEdited { get; }

        IObservable<ChatSummaryModel> ChatAdded { get; }
        IObservable<string> ChatRemoved { get; }
        IObservable<(string ChatId, bool IsMuted)> IsChatMutedChanged { get; }
        IObservable<(string ChatId, int NewCount)> UnreadMessageCountChanged { get; }

        IObservable<SocketConnectionStatus> ConnectionStatusChanged { get; }
        SocketConnectionStatus ConnectionStatus { get; }

        Task<IList<ChatSummaryModel>> GetChatsHeadersAsync();
        Task<ChatSummaryModel> CreateChatAsync(string chatName, IList<string> participantsIds, string imagePath);
        Task CloseChatAsync(string chatId);
        Task LeaveChatAsync(string chatId);
        Task InviteMembersAsync(string chatId, IList<string> participantsIds);

        Task<IList<ChatMessageModel>> GetOlderMessagesAsync(string chatId,
                                                            string messageFromId = null,
                                                            DateTimeOffset? messageFromDateTime = null,
                                                            int? count = null);
        Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId);
        Task<IList<ChatMessageModel>> GetMessagesFromAsync(string chatId,
                                                           string messageFromId,
                                                           DateTimeOffset messageFromDateTime,
                                                           int? count = null);
        Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId);

        Task MarkMessageAsReadAsync(string chatId, string messageId);
        Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody, string imageUrl);
        Task EditMessageAsync(string messageId, string messageBody);
        Task DeleteMessageAsync(string chatId, string messageId);

        Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId);
        
        Task<PagingModel<ChatUserModel>> GetContactsAsync(string nameFilter, int pageNumber, int pageSize);
        
        Task<PagingModel<ChatUserModel>> GetContactsForInviteAsync(string chatId,
            string nameFilter, int pageNumber, int pageSize);

        void ForceReconnect();
        void ForceDisconnect();

        Task EditChatAsync(ChatSummaryModel chatSummary);
        void Logout();
    }
}
