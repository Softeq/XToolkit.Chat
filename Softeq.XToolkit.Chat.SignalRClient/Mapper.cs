// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.ComponentModel;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Message;

namespace Softeq.XToolkit.Chat.SignalRClient
{
    internal static class Mapper
    {
        public static ChatSummaryModel DtoToChatSummary(ChannelSummaryResponse response)
        {
            if (response == null)
            {
                return null;
            }
            var lastMessage = DtoToChatMessage(response.LastMessage);
            return new ChatSummaryModel
            {
                Id = response.Id.ToString(),
                Name = response.Name,
                AvatarUrl = response.PhotoUrl,
                LastMessage = lastMessage,
                IsMuted = response.IsMuted,
                CreatedDate = response.Created,
                UpdatedDate = response.Updated,
                UnreadMessagesCount = response.UnreadMessagesCount,
                CreatorId = response.Creator?.Id.ToString()
            };
        }

        public static ChatMessageModel DtoToChatMessage(MessageResponse response)
        {
            if (response == null)
            {
                return null;
            }
            return new ChatMessageModel
            {
                Id = response.Id.ToString(),
                Body = response.Body,
                ChannelId = response.ChannelId.ToString(),
                DateTime = response.Created,
                SenderId = response.Sender?.Id.ToString(),
                SenderName = response.Sender?.UserName,
                SenderPhotoUrl = response.Sender?.AvatarUrl,
                MessageType = DtoToMessageType(response.Type),
                IsRead = response.IsRead,
                IsDelivered = true,
                ImageUrl = response.ImageUrl
            };
        }

        public static Models.MessageType DtoToMessageType(DTOs.Message.MessageType dto)
        {
            switch (dto)
            {
                case DTOs.Message.MessageType.Default:
                    return Models.MessageType.Default;
                case DTOs.Message.MessageType.Notification:
                    return Models.MessageType.Info;
                default: throw new InvalidEnumArgumentException();
            }
        }
    }
}
