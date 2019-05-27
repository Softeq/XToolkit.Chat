// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Exceptions;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.WhiteLabel.ImagePicker;

namespace Softeq.XToolkit.Chat.Manager
{
    public partial class ChatManager : IChatMessagesManager
    {
        public event EventHandler<int> TotalUnreadMessagesCountChange;

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

                // TODO YP: check recursion
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
            var models = await _messagesCache.GetOlderMessagesAsync(chatId,
                messageFromId, messageFromDateTime, count).ConfigureAwait(false);

            if (models.Count > 0)
            {
                return CreateMessagesViewModels(models);
            }

            var olderMessagesModels = await _chatService.GetOlderMessagesAsync(chatId,
                messageFromId, messageFromDateTime, count).ConfigureAwait(false);

            if (olderMessagesModels != null && olderMessagesModels.Any())
            {
                await _messagesCache.SaveMessagesAsync(chatId, olderMessagesModels).ConfigureAwait(false);

                // TODO YP: check recursion
                return await LoadOlderMessagesAsync(chatId, messageFromId, messageFromDateTime, count).ConfigureAwait(false);
            }
            return new List<ChatMessageViewModel>();
        }

        public async Task SendMessageAsync(string chatId, string messageBody, ImagePickerArgs imagePickerArgs)
        {
            var tempMessage = new ChatMessageModel
            {
                DateTime = DateTimeOffset.UtcNow,
                Body = messageBody,
                ChannelId = chatId,
                IsMine = true,
                ImageCacheKey = imagePickerArgs?.ImageCacheKey
            };

            var tempMessageViewModel = CreateAndAddNewTempMessage(tempMessage);

            var imageUrl = await UploadImageAsync(imagePickerArgs);

            var sentMessage = await _chatService.SendMessageAsync(chatId, messageBody, imageUrl).ConfigureAwait(false);

            if (sentMessage != null)
            {
                TryUpdateTempMessageAfterSend(tempMessageViewModel, sentMessage);
            }
        }

        // TODO YP: need change current approach for upload photo (before sending message)
        // Better way:
        // - start upload immediately after select
        // - when message canceled - send request for remove temp image
        private async Task<string> UploadImageAsync(ImagePickerArgs imagePickerArgs)
        {
            var imageUrl = default(string);

            if (imagePickerArgs != null)
            {
                try
                {
                    imageUrl = await _uploadImageService
                        .UploadImageAsync(() => (imagePickerArgs.ImageStream(), imagePickerArgs.Extension))
                        .ConfigureAwait(false);
                }
                catch (InvalidDataException ex)
                {
                    _logger.Error(ex);
                }
            }

            return imageUrl;
        }

        private void TryUpdateTempMessageAfterSend(ChatMessageViewModel tempMessageViewModel,
            ChatMessageModel sentMessage)
        {
            try
            {
                _messagesCache.UpdateSentMessage(tempMessageViewModel.Model, sentMessage);

                tempMessageViewModel.UpdateMessageModel(sentMessage);
            }
            catch (ChatCacheException e)
            {
                _logger.Error(e);
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

        public async Task MarkMessageAsReadAsync(string chatId, string messageId)
        {
            if (string.IsNullOrEmpty(chatId) || string.IsNullOrEmpty(messageId))
            {
                return;
            }

            await _chatService.MarkMessageAsReadAsync(chatId, messageId).ConfigureAwait(false);
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

        private void OnChatRead(string chatId)
        {
            _messagesCache.ReadMyLatestMessages(chatId);
        }

        private ChatMessageViewModel CreateAndAddNewTempMessage(ChatMessageModel message)
        {
            var messageViewModel = CreateMessageViewModel(message);

            if (TryAddMessageToCache(message))
            {
                _messageAdded.OnNext(messageViewModel);

                FindChatForUpdateLastMessage(message);
            }

            return messageViewModel;
        }

        private void AddLatestMessage(ChatMessageModel message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var messageViewModel = CreateMessageViewModel(message);

            if (TryAddMessageToCache(message))
            {
                _messageAdded.OnNext(messageViewModel);

                var chat = new ChatSummaryModel { Id = messageViewModel.ChatId };

                if (TryAddChat(chat))
                {
                    UpdateChatsListFromNetworkAsync();
                }

                FindChatForUpdateLastMessage(message);
            }
        }

        private bool TryAddMessageToCache(ChatMessageModel message)
        {
            if (_messagesCache.FindDuplicateMessage(message) == null)
            {
                _messagesCache.TryAddMessage(message);
                return true;
            }
            return false;
        }

        private void FindChatForUpdateLastMessage(ChatMessageModel messageModel)
        {
            ModifyChatsSafely(() =>
            {
                var chatViewModel = GetChatById(messageModel.ChannelId);

                if (chatViewModel != null)
                {
                    chatViewModel.UpdateLastMessage(messageModel);

                    if (messageModel.IsMine)
                    {
                        chatViewModel.UnreadMessageCount = 0;
                    }
                    else
                    {
                        chatViewModel.UnreadMessageCount++;
                    }
                }
                ChatsCollection.Sort((x, y) => y.LastUpdateDate.CompareTo(x.LastUpdateDate));
            });
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
