// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.ComponentModel;
using Softeq.NetKit.Chat.SignalRClient.DTOs.Channel.Response;
using Softeq.NetKit.Chat.SignalRClient.DTOs.Member;
using Softeq.NetKit.Chat.SignalRClient.DTOs.Member.Response;
using Softeq.NetKit.Chat.SignalRClient.DTOs.Message;
using Softeq.XToolkit.Chat.Models;
using MessageType = Softeq.NetKit.Chat.SignalRClient.DTOs.Message.MessageType;

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
                PhotoUrl = response.PhotoUrl,
                LastMessage = lastMessage,
                IsMuted = response.IsMuted,
                CreatedDate = response.Created,
                UpdatedDate = response.Updated,
                UnreadMessagesCount = response.UnreadMessagesCount,
                CreatorId = response.Creator?.Id.ToString(),
                Type = (ChannelType)response.Type,
                Creator = DtoToChatUser(response.Creator),
                DirectMember = DtoToChatUser(response.DirectMember)
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
                ImageRemoteUrl = response.ImageUrl,
                ChannelType = (ChannelType)response.ChannelType
            };
        }

        private static Models.MessageType DtoToMessageType(MessageType dto)
        {
            switch (dto)
            {
                case MessageType.Default:
                    return Models.MessageType.Default;
                case MessageType.Notification:
                    return Models.MessageType.Info;
                default: throw new InvalidEnumArgumentException();
            }
        }

        private static ChatUserModel DtoToChatUser(MemberSummaryResponse dto)
        {
            return dto == null ? null : new ChatUserModel
            {
                Id = dto.Id.ToString(),
                Username = dto.UserName,
                PhotoUrl = dto.AvatarUrl,
                LastActivity = dto.LastActivity,
                IsOnline = dto.Status == UserStatus.Online,
                IsActive = dto.IsActive
            };
        }
    }
}
