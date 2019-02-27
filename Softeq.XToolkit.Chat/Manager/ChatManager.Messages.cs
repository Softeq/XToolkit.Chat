// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Exceptions;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.WhiteLabel.ImagePicker;

namespace Softeq.XToolkit.Chat.Manager
{
    public partial class ChatManager
    {
        public async Task<IList<ChatMessageViewModel>> LoadInitialMessagesAsync(string chatId, int count)
        {
            var models = await _messagesCache.GetLatestMessagesAsync(chatId, count).ConfigureAwait(false);
            if (models.Count > 0)
            {
                return CreateMessagesViewModels(models);
            }
            var olderMessagesModels = await _chatService.GetLatestMessagesAsync(chatId).ConfigureAwait(false);
            if (olderMessagesModels != null && olderMessagesModels.Any())
            {
                await _messagesCache.SaveMessagesAsync(chatId, olderMessagesModels).ConfigureAwait(false);
                return await LoadInitialMessagesAsync(chatId, count).ConfigureAwait(false);
            }
            return new List<ChatMessageViewModel>();
        }

        public async Task<IList<ChatMessageViewModel>> LoadOlderMessagesAsync(
            string chatId,
            string messageFromId,
            DateTimeOffset messageFromDateTime,
            int count)
        {
            var models = await _messagesCache.GetOlderMessagesAsync(chatId, messageFromId, messageFromDateTime, count)
                                             .ConfigureAwait(false);
            if (models.Count > 0)
            {
                return CreateMessagesViewModels(models);
            }
            var olderMessagesModels = await _chatService.GetOlderMessagesAsync(chatId, messageFromId, messageFromDateTime, count)
                                                        .ConfigureAwait(false);
            if (olderMessagesModels != null && olderMessagesModels.Any())
            {
                await _messagesCache.SaveMessagesAsync(chatId, olderMessagesModels).ConfigureAwait(false);
                return await LoadOlderMessagesAsync(chatId, messageFromId, messageFromDateTime, count).ConfigureAwait(false);
            }
            return new List<ChatMessageViewModel>();
        }

        public async Task SendMessageAsync(string chatId, string messageBody, ImagePickerArgs imagePickerArgs)
        {
            var messageModel = new ChatMessageModel
            {
                DateTime = DateTimeOffset.UtcNow,
                Body = messageBody,
                ChannelId = chatId,
                IsMine = true,
                ImageCacheKey = imagePickerArgs?.ImageCacheKey
            };

            var viewModel = AddLatestMessage(messageModel);

            var imageUrl = default(string);

            if (imagePickerArgs != null)
            {
                imageUrl = await _uploadImageService
                    .UploadImageAsync(() => (imagePickerArgs.ImageStream(), imagePickerArgs.Extension))
                    .ConfigureAwait(false);
            }

            var updatedModel = await _chatService.SendMessageAsync(chatId, messageBody, imageUrl).ConfigureAwait(false);
            if (updatedModel != null)
            {
                try
                {
                    _messagesCache.UpdateSentMessage(messageModel, updatedModel);
                }
                catch (ChatCacheException e)
                {
                    _logger.Error(e);
                }

                viewModel.UpdateMessageModel(updatedModel);
            }
        }

        public Task EditMessageAsync(string messageId, string messageBody)
        {
            return _chatService.EditMessageAsync(messageId, messageBody);
        }

        public Task DeleteMessageAsync(string chatId, string messageId)
        {
            return _chatService.DeleteMessageAsync(chatId, messageId);
        }

        public async Task MarkMessageAsReadAsync(string messageId, ChatSummaryViewModel chatSummary)
        {
            if (messageId == null)
            {
                return;
            }
            if (chatSummary != null)
            {
                chatSummary.UnreadMessageCount = 0;
            }

            await _chatService.MarkMessageAsReadAsync(chatSummary.ChatId, messageId).ConfigureAwait(false);
        }

        private void OnMessageAdded(ChatMessageModel chatMessage)
        {
            AddLatestMessage(chatMessage);
        }

        private void OnMessageEdited(ChatMessageModel updatedMessage)
        {
            if (updatedMessage == null)
            {
                return;
            }
            _messagesCache.TryEditMessage(updatedMessage);
            _messageEdited.OnNext(updatedMessage);
            ModifyChatsSafely(() =>
            {
                var chatSummary = ChatsCollection.FirstOrDefault(x => x.ChatId == updatedMessage.ChannelId);
                if (chatSummary != null)
                {
                    chatSummary.UpdateLastMessage(updatedMessage);
                }
            });
        }

        private void OnMessageDeleted((string DeletedMessageId, ChatSummaryModel UpdatedChatSummary) value)
        {
            if (value.DeletedMessageId == null || value.UpdatedChatSummary == null)
            {
                return;
            }
            _messagesCache.TryDeleteMessage(value.UpdatedChatSummary.Id, value.DeletedMessageId);
            _messageDeleted.OnNext(value.DeletedMessageId);
            ModifyChatsSafely(() =>
            {
                var chatSummary = ChatsCollection.FirstOrDefault(x => x.ChatId == value.UpdatedChatSummary.Id);
                if (chatSummary != null)
                {
                    // TODO: [Backend] UpdatedChatSummary.LastMessage is null
                    chatSummary.UpdateLastMessage(value.UpdatedChatSummary.LastMessage);
                    chatSummary.UnreadMessageCount = value.UpdatedChatSummary.UnreadMessagesCount;
                }
            });
        }

        private ChatMessageViewModel AddLatestMessage(ChatMessageModel messageModel)
        {
            if (messageModel == null)
            {
                throw new ArgumentNullException(nameof(messageModel));
            }

            var messageViewModel = CreateMessageViewModel(messageModel);

            if (_messagesCache.FindDuplicateMessage(messageModel) != null)
            {
                return messageViewModel;
            }

            _messagesCache.TryAddMessage(messageModel);

            _messageAdded.OnNext(messageViewModel);

            if (TryAddChat(new ChatSummaryModel { Id = messageViewModel.ChatId }))
            {
                UpdateChatsListFromNetworkAsync();
            }
            ModifyChatsSafely(() =>
            {
                foreach (var chatSummary in ChatsCollection.Where(x => x.ChatId == messageViewModel.ChatId))
                {
                    chatSummary.UpdateLastMessage(messageModel);
                    if (messageModel.IsMine)
                    {
                        chatSummary.UnreadMessageCount = 0;
                    }
                    else
                    {
                        chatSummary.UnreadMessageCount++;
                    }
                }
                ChatsCollection.Sort((x, y) => y.LastUpdateDate.CompareTo(x.LastUpdateDate));
            });
            return messageViewModel;
        }

        private Task UpdateMessagesCacheAsync()
        {
            var chatsIds = ChatsCollection.Select(x => x.ChatId).ToList();
            return _messagesCache.PerformFullUpdate(chatsIds);
        }

        private IList<ChatMessageViewModel> CreateMessagesViewModels(IList<ChatMessageModel> models)
        {
            return models?.Select(CreateMessageViewModel).ToList();
        }

        private ChatMessageViewModel CreateMessageViewModel(ChatMessageModel model)
        {
            var viewModel = _viewModelFactoryService.ResolveViewModel<ChatMessageViewModel>();
            viewModel.UpdateMessageModel(model);
            return viewModel;
        }
    }
}
