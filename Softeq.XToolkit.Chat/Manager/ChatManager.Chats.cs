﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Extensions;

namespace Softeq.XToolkit.Chat.Manager
{
    public partial class ChatManager : IChatsListManager
    {
        public void MakeChatActive(string chatId)
        {
            _activeChatId = chatId;
        }

        private Task UpdateChatsListFromNetworkAsync()
        {
            return UpdateChatsListWithLoader(GetChatsListAsync());
        }

        private async Task UpdateChatsListWithLoader(Task<IList<ChatSummaryModel>> loader)
        {
            var result = await loader.ConfigureAwait(false);

            if (result != null)
            {
                var orderedChats = result
                    .Select(_viewModelFactoryService.ResolveViewModel<ChatSummaryViewModel, ChatSummaryModel>)
                    .OrderByDescending(x => x.LastUpdateDate)
                    .ToList();

                ModifyChatsSafely(() =>
                {
                    ChatsCollection.ReplaceRange(orderedChats);
                });
            }
        }

        private async Task<IList<ChatSummaryModel>> GetChatsListAsync()
        {
            var models = await _chatService.GetChatsListAsync().ConfigureAwait(false);
            if (models == null)
            {
                return null;
            }

            await _localCache.Add(ChatsCacheKey, DateTimeOffset.UtcNow, models).ConfigureAwait(false);

            return models;
        }

        public void RefreshChatsListOnBackgroundAsync()
        {
            if (ChatsCollection.Count == 0)
            {
                UpdateChatsListWithLoader(_localCache.Get<IList<ChatSummaryModel>>(ChatsCacheKey)).FireAndForget();
            }

            UpdateChatsListFromNetworkAsync().FireAndForget();
        }

        public async Task<bool> CreateChatAsync(string chatName, IList<string> participantsIds, string imagePath)
        {
            var chatModel = await _chatService.CreateChatAsync(chatName, participantsIds, imagePath).ConfigureAwait(false);
            if (chatModel != null)
            {
                TryAddChat(chatModel);

                return true;
            }
            return false;
        }

        public Task CloseChatAsync(string chatId)
        {
            return _chatService.CloseChatAsync(chatId);
        }

        public Task LeaveChatAsync(string chatId)
        {
            return _chatService.LeaveChatAsync(chatId);
        }

        public Task MuteChatAsync(string chatId)
        {
            return _chatService.MuteChatAsync(chatId);
        }

        public Task UnMuteChatAsync(string chatId)
        {
            return _chatService.UnMuteChatAsync(chatId);
        }

        public Task InviteMembersAsync(string chatId, IList<string> participantsIds)
        {
            return _chatService.InviteMembersAsync(chatId, participantsIds);
        }

        public async Task<IList<ChatUserViewModel>> GetChatMembersAsync(string chatId)
        {
            var models = await _chatService.GetChatMembersAsync(chatId).ConfigureAwait(false);
            return models?.Select(x => new ChatUserViewModel { Parameter = x }).ToList();
        }

        public Task EditChatAsync(ChatSummaryModel chatSummary)
        {
            return _chatService.EditChatAsync(chatSummary);
        }

        public ChatSummaryViewModel GetChatById(string chatId)
        {
            return ChatsCollection.FirstOrDefault(x => x.ChatId == chatId);
        }

        public async Task<ChatSummaryViewModel> GetChatByIdFromCacheAsync(string chatId)
        {
            if (ChatsCollection.Count == 0)
            {
                await UpdateChatsListWithLoader(_localCache.Get<IList<ChatSummaryModel>>(ChatsCacheKey));
            }

            return GetChatById(chatId);
        }

        public async Task<ChatSummaryViewModel> FindOrCreateDirectChatAsync(string memberId)
        {
            var directChatWithMember = ChatsCollection.FirstOrDefault(x =>
                x.Parameter.Type == ChannelType.Direct &&
                x.Parameter.DirectMember.Id == memberId);

            if (directChatWithMember != null)
            {
                return directChatWithMember;
            }

            var directChatModel = await _chatService.CreateDirectChatAsync(memberId).ConfigureAwait(false);

            if (directChatModel == null)
            {
                return null;
            }

            var directChatViewModel = _viewModelFactoryService.ResolveViewModel<ChatSummaryViewModel, ChatSummaryModel>(directChatModel);

            return directChatViewModel;
        }

        private bool TryAddChat(ChatSummaryModel chatSummary)
        {
            return ModifyChatsSafely(() =>
            {
                if (chatSummary == null || ChatsCollection.Any(x => x.ChatId == chatSummary.Id))
                {
                    return false;
                }
                var viewModel = _viewModelFactoryService.ResolveViewModel<ChatSummaryViewModel, ChatSummaryModel>(chatSummary);
                ChatsCollection.Insert(0, viewModel);
                ChatsCollection.Sort((x, y) => y.LastUpdateDate.CompareTo(x.LastUpdateDate));
                return true;
            });
        }

        private void OnChatRemoved(string chatId)
        {
            ModifyChatsSafely(() =>
            {
                var chatSummary = ChatsCollection.FirstOrDefault(x => x.ChatId == chatId);
                if (chatSummary != null)
                {
                    ChatsCollection.Remove(chatSummary);
                }
            });

            _messagesCache.RemoveMessagesAsync(chatId).FireAndForget();
        }

        private void ModifyChatsSafely(Action modifyAction)
        {
            if (modifyAction == null)
            {
                return;
            }
            ModifyChatsSafely(() =>
            {
                modifyAction();
                return true;
            });
        }

        private T ModifyChatsSafely<T>(Func<T> modifyAction)
        {
            if (modifyAction == null)
            {
                return default(T);
            }
            lock (ChatsCollection)
            {
                return modifyAction();
            }
        }
    }
}
