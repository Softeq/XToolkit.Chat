using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.Interfaces
{
    public interface IChatMessagesManager
    {
        IObservable<ChatMessageViewModel> MessageAdded { get; }
        IObservable<ChatMessageModel> MessageEdited { get; }
        IObservable<string> MessageDeleted { get; }
        IObservable<IList<ChatMessageViewModel>> MessagesBatchAdded { get; }
        IObservable<IList<ChatMessageModel>> MessagesBatchUpdated { get; }
        IObservable<IList<string>> MessagesBatchDeleted { get; }

        Task<IList<ChatMessageViewModel>> LoadInitialMessagesAsync(string chatId, int count);

        Task<IList<ChatMessageViewModel>> LoadOlderMessagesAsync(
            string chatId,
            string messageFromId,
            DateTimeOffset messageFromDateTime,
            int count);

        Task MarkMessageAsReadAsync(string chatId, string messageId);
    }
}
