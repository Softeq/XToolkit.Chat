// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Collections;
using TaskExtensions = Softeq.XToolkit.Common.Extensions.TaskExtensions;

namespace Softeq.XToolkit.Chat.Manager
{
    public partial class ChatManager
    {
        public async Task UpdateChatsListAsync()
        {
            var result = await GetChatsListAsync();

            if (result != null)
            {
                ModifyChatsSafely(() => ChatsCollection.ReplaceRange(result.OrderByDescending(x => x.LastUpdateDate)));
            }
        }

        public void MakeChatActive(string chatId)
        {
            _activeChatId = chatId;
        }

        public async Task<IList<ChatSummaryViewModel>> GetChatsListAsync()
        {
            var models = await _chatService.GetChatsHeadersAsync().ConfigureAwait(false);
            return models?.Select(_viewModelFactoryService.ResolveViewModel<ChatSummaryViewModel, ChatSummaryModel>)?.ToList();
        }

        public async Task CreateChatAsync(string chatName, IList<string> participantsIds, string imagePath)
        {
            var chatModel = await _chatService.CreateChatAsync(chatName, participantsIds, imagePath).ConfigureAwait(false);
            if (chatModel != null)
            {
                TryAddChat(chatModel);
            }
        }

        public Task CloseChatAsync(string chatId)
        {
            return _chatService.CloseChatAsync(chatId);
        }

        public Task LeaveChatAsync(string chatId)
        {
            return _chatService.LeaveChatAsync(chatId);
        }

        public Task InviteMembersAsync(string chatId, IList<string> participantsIds)
        {
            return _chatService.InviteMembersAsync(chatId, participantsIds);
        }

        public async Task<IList<ChatUserViewModel>> GetContactsAsync(string nameFilter, int pageNumber, int pageSize)
        {
            var models = await _chatService.GetContactsAsync(nameFilter, pageNumber, pageSize).ConfigureAwait(false);

            // TODO YP: what about put to cache, after that create VM on target VM, not this?
            
            if (models == null)
            {
                return new List<ChatUserViewModel>();
            }
            
            return models.Data
                .Select(_viewModelFactoryService.ResolveViewModel<ChatUserViewModel, ChatUserModel>)
                .ToList();
        }

        public async Task<IList<ChatUserViewModel>> GetChatMembersAsync(string chatId)
        {
            var models = await _chatService.GetChatMembersAsync(chatId).ConfigureAwait(false);
            return models?.Select(_viewModelFactoryService.ResolveViewModel<ChatUserViewModel, ChatUserModel>)?.ToList();
        }

        internal Task EditChatAsync(ChatSummaryModel chatSummary)
        {
            return _chatService.EditChatAsync(chatSummary);
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

            TaskExtensions.SafeTaskWrapper(_messagesCache.RemoveMessagesAsync(chatId));
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
