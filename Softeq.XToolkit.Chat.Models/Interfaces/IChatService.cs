// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Queries;
using Softeq.XToolkit.Common.Models;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IChatService
    {
        IObservable<ChatMessageModel> MessageReceived { get; }
        IObservable<ChatDeletedMessageModel> MessageDeleted { get; }
        IObservable<ChatMessageModel> MessageEdited { get; }

        IObservable<ChatSummaryModel> ChatAdded { get; }
        IObservable<string> ChatRemoved { get; }
        IObservable<ChatSummaryModel> ChatUpdated { get; }
        IObservable<string> ChatRead { get; }
        IObservable<(string ChatId, bool IsMuted)> IsChatMutedChanged { get; }
        IObservable<(string ChatId, int NewCount)> UnreadMessageCountChanged { get; }

        IObservable<SocketConnectionStatus> ConnectionStatusChanged { get; }
        SocketConnectionStatus ConnectionStatus { get; }

        Task<IList<ChatSummaryModel>> GetChatsListAsync();
        Task<ChatSummaryModel> CreateChatAsync(string chatName, IList<string> participantsIds, string imagePath);
        Task<ChatSummaryModel> CreateDirectChatAsync(string memberId);
        Task EditChatAsync(ChatSummaryModel chatSummary);
        Task CloseChatAsync(string chatId);
        Task LeaveChatAsync(string chatId);
        Task InviteMembersAsync(string chatId, IList<string> participantsIds);
        Task DeleteMemberAsync(string chatId, string memberId);
        Task MuteChatAsync(string chatId);
        Task UnMuteChatAsync(string chatId);

        Task<IList<ChatMessageModel>> GetOlderMessagesAsync(MessagesQuery query);
        Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId);
        Task<IList<ChatMessageModel>> GetMessagesFromAsync(MessagesQuery query);
        Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId);

        Task MarkMessageAsReadAsync(string chatId, string messageId);
        Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody, string imageUrl);
        Task EditMessageAsync(string messageId, string messageBody);
        Task DeleteMessageAsync(string chatId, string messageId);

        Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId);

        Task<PagingModel<ChatUserModel>> GetContactsAsync(string nameFilter, int pageNumber, int pageSize);

        Task<PagingModel<ChatUserModel>> GetContactsForInviteAsync(ContactsQuery query);

        void ForceReconnect();
        void ForceDisconnect();

        void Logout();
    }
}
