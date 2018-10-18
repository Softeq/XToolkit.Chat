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
        IObservable<(string DeletedMessageId, ChatSummaryModel UpdatedChatSummary)> MessageDeleted { get; }
        IObservable<string> MessageRead { get; }
        IObservable<ChatMessageModel> MessageEdited { get; }

        IObservable<ChatSummaryModel> ChatAdded { get; }
        IObservable<string> ChatRemoved { get; }
        IObservable<(string ChatId, bool IsMuted)> IsChatMutedChanged { get; }
        IObservable<(string ChatId, int NewCount)> UnreadMessageCountChanged { get; }

        IObservable<SocketConnectionStatus> ConnectionStatusChanged { get; }
        SocketConnectionStatus ConnectionStatus { get; }

        Task<ChatSummaryModel> CreateChatAsync(string chatName, IList<string> participantsIds);
        Task CloseChatAsync(string chatId);
        Task LeaveChatAsync(string chatId);
        Task InviteMembersAsync(string chatId, IList<string> participantsIds);

        Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody);
        Task EditMessageAsync(string messageId, string newBody);
        Task DeleteMessageAsync(string channelId, string messageId);

        void ForceReconnect();
        void ForceDisconnect();
    }
}
