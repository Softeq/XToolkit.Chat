// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Chat.Models;
using System.ComponentModel;
using System.Linq;
using Softeq.XToolkit.Common.Models;
using Softeq.XToolkit.RemoteData;

namespace Softeq.XToolkit.Chat.HttpClient
{
    internal static class Mapper
    {
        public static ChatSummaryModel DtoToChatSummary(ChatSummaryDto dto)
        {
            return dto == null ? null : new ChatSummaryModel
            {
                Id = dto.Id,
                CreatedDate = dto.Created,
                UpdatedDate = dto.Updated,
                UnreadMessagesCount = dto.UnreadMessagesCount,
                Name = dto.Name,
                IsClosed = dto.IsClosed,
                IsMuted = dto.IsMuted,
                IsPinned = dto.IsPinned,
                CreatorId = dto.Creator?.Id,
                DirectMember = DtoToChatUser(dto.DirectMember),
                Description = dto.Description,
                WelcomeMessage = dto.WelcomeMessage,
                Type = (ChannelType)dto.Type,
                LastMessage = DtoToChatMessage(dto.LastMessage),
                PhotoUrl = dto.PhotoUrl
            };
        }

        public static ChatMessageModel DtoToChatMessage(ChatMessageDto dto)
        {
            return dto == null ? null : new ChatMessageModel
            {
                Id = dto.Id.ToString(),
                ChannelId = dto.ChannelId,
                SenderId = dto.Sender?.Id,
                SenderName = dto.Sender?.UserName,
                SenderPhotoUrl = dto.Sender?.AvatarUrl,
                MessageType = DtoToMessageType(dto.Type),
                Body = dto.Body,
                DateTime = dto.Created,
                IsRead = dto.IsRead,
                IsDelivered = true,
                ImageRemoteUrl = dto.ImageUrl,
                ChannelType = (ChannelType)dto.ChannelType
            };
        }

        public static ChatUserModel DtoToChatUser(ChatUserDto dto)
        {
            return dto == null ? null : new ChatUserModel
            {
                Id = dto.Id,
                Username = dto.UserName,
                PhotoUrl = dto.AvatarUrl,
                LastActivity = dto.LastActivity,
                IsOnline = dto.Status == ChatUserStatusDto.Online,
                IsActive =dto.IsActive
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
                    return MessageType.Unknown;
                    //throw new InvalidEnumArgumentException("messageType", (int)dto, typeof(MessageTypeDto));
            }
        }

        public static PagingModel<ChatUserModel> PagedMembersDtoToPagingModel(PagingModelDto<ChatUserDto> dto)
        {
            if (dto?.Items == null)
            {
                return null;
            }

            var items = dto.Items
                .Where(y => y.AvatarUrl != null || !string.IsNullOrEmpty(y.UserName))
                .Select(DtoToChatUser)
                .ToList();

            return new PagingModel<ChatUserModel>
            {
                Page = dto.PageNumber,
                Data = items,
                TotalNumberOfPages = dto.TotalNumberOfPages,
                TotalNumberOfRecords = dto.TotalNumberOfRecords,
                PageSize = dto.PageSize
            };
        }
    }
}
