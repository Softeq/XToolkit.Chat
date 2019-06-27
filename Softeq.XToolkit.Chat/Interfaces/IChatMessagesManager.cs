// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Queries;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.Interfaces
{
    public interface IChatMessagesManager
    {
        event EventHandler<int> TotalUnreadMessagesCountChange;

        IObservable<ChatMessageViewModel> MessageAdded { get; }
        IObservable<ChatMessageModel> MessageEdited { get; }
        IObservable<string> MessageDeleted { get; }
        IObservable<IList<ChatMessageViewModel>> MessagesBatchAdded { get; }
        IObservable<IList<ChatMessageModel>> MessagesBatchUpdated { get; }
        IObservable<IList<string>> MessagesBatchDeleted { get; }

        IObservable<string> ChatRead { get; }

        Task<IList<ChatMessageViewModel>> LoadInitialMessagesAsync(string chatId, int count);

        Task<IList<ChatMessageViewModel>> LoadOlderMessagesAsync(MessagesQuery query);

        Task MarkMessageAsReadAsync(string chatId, string messageId);
    }
}
