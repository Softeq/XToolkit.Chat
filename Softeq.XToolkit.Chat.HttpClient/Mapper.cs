// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Chat.Models;
using System.ComponentModel;

namespace Softeq.XToolkit.Chat.HttpClient
{
    internal static class Mapper
    {
        public static ChatSummaryModel DtoToChatSummary(ChatSummaryDto dto)
        {
            return dto == null ? null : new ChatSummaryModel
            {
                Id = dto.Id,
                Name = dto.Name,
                UnreadMessagesCount = dto.UnreadMessagesCount,
                IsMuted = dto.IsMuted,
                CreatorId = dto.CreatorSaasUserId,
                AvatarUrl = dto.PhotoUrl,
                LastMessage = DtoToChatMessage(dto.LastMessage),
                CreatedDate = dto.Created,
                UpdatedDate = dto.Updated
            };
        }

        public static ChatMessageModel DtoToChatMessage(ChatMessageDto dto)
        {
            return dto == null ? null : new ChatMessageModel
            {
                Id = dto.Id.ToString(),
                ChannelId = dto.ChannelId,
                SenderId = dto.Sender?.SaasUserId,
                SenderName = dto.Sender?.UserName,
                SenderPhotoUrl = dto.Sender?.AvatarUrl,
                MessageType = DtoToMessageType(dto.Type),
                Body = dto.Body,
                DateTime = dto.Created,
                IsRead = dto.IsRead,
                IsDelivered = true,
                ImageUrl = dto.ImageUrl
            };
        }

        public static ChatUserModel DtoToChatUser(ChatUserDto dto)
        {
            return dto == null ? null : new ChatUserModel
            {
                Id = dto.Id,
                Username = dto.UserName,
                PhotoUrl = dto.AvatarUrl,
                SaasUserId = dto.SaasUserId,
                LastActivity = dto.LastActivity,
                IsOnline = !dto.IsAfk
            };
        }

        public static MessageType DtoToMessageType(MessageTypeDto dto)
        {
            switch (dto)
            {
                case MessageTypeDto.Default:
                    return MessageType.Default;
                case MessageTypeDto.Notification:
                    return MessageType.Info;
                default:
                    throw new InvalidEnumArgumentException("messageType", (int)dto, typeof(MessageTypeDto));
            }
        }
    }
}
