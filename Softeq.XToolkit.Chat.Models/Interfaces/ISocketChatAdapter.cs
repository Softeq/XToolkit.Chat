// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models.Enum;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface ISocketChatAdapter
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

        Task<ChatSummaryModel> CreateChatAsync(string chatName, IList<string> participantsIds, string chatAvatar);
        Task<ChatSummaryModel> CreateDirectChatAsync(string memberId);
        Task EditChatAsync(ChatSummaryModel chatSummary);
        Task CloseChatAsync(string chatId);
        Task LeaveChatAsync(string chatId);
        Task InviteMembersAsync(string chatId, IList<string> participantsIds);
        Task DeleteMemberAsync(string chatId, string memberId);

        Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody, string imageUrl);
        Task EditMessageAsync(string messageId, string newBody);
        Task DeleteMessageAsync(string channelId, string messageId);

        void ForceReconnect();
        void ForceDisconnect();
    }
}
