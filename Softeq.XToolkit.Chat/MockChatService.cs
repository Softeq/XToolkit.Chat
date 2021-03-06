﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.Models.Queries;
using Softeq.XToolkit.Common.Logger;

namespace Softeq.XToolkit.Chat
{
    public class MockChatService : ChatService
    {
        public MockChatService(ISocketChatAdapter socketChatAdapter,
                               IHttpChatAdapter httpChatAdapter,
                               ILogManager logManager) : base(socketChatAdapter, httpChatAdapter, logManager)
        {
        }

        public override async Task<IList<ChatSummaryModel>> GetChatsListAsync()
        {
            await Task.Delay(500);
            return new List<ChatSummaryModel> { new ChatSummaryModel { Id = "1", Name = "test" } };
        }

        /// <summary>
        ///     Pass current chat messages count instead of page size.
        /// </summary>
        public override async Task<IList<ChatMessageModel>> GetOlderMessagesAsync(MessagesQuery query)
        {
            await Task.Yield();
            //await Task.Delay(1000);
            int messagesCount = 100;
            var testMessages = new List<ChatMessageModel>();
            var l = new List<int>();
            for (int i = query.Count.Value; i < query.Count.Value + messagesCount; i++)
            {
                l.Add(i);
            }
            for (int i = query.Count.Value; i < query.Count.Value + messagesCount; i++)
            {
                var hoursOffset = l[i - query.Count.Value];
                var time = DateTimeOffset.Now.AddHours(-hoursOffset);
                testMessages.Add(new ChatMessageModel
                {
                    Id = i.ToString(),
                    Body = i.ToString(),
                    DateTime = time
                });
            }
            return testMessages;
        }

        public override async Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId)
        {
            await Task.Delay(1000);
            int messagesCount = 20;
            var testMessages = new List<ChatMessageModel>();
            var l = new List<int>();
            for (int i = 0; i < messagesCount; i++)
            {
                l.Insert(0, i);
            }
            for (int i = 0; i < messagesCount; i++)
            {
                var hoursOffset = l[i];
                var time = DateTimeOffset.Now.AddHours(-i);
                testMessages.Add(new ChatMessageModel
                {
                    Id = i.ToString(),
                    Body = i.ToString(),
                    DateTime = time
                });
            };
            return testMessages;
        }
    }
}
