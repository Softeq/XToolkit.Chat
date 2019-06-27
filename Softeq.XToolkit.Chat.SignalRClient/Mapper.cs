// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.ComponentModel;
using System.Linq;
using Softeq.XToolkit.Chat.Models;
using Softeq.NetKit.Chat.TransportModels.Enums;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Channel;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Member;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Message;
using MessageType = Softeq.NetKit.Chat.TransportModels.Enums.MessageType;

namespace Softeq.XToolkit.Chat.SignalRClient
{
    internal static class Mapper
    {
        #region Common Mapper Methods - HttpClient.Mapper

        internal static ChatSummaryModel DtoToChatSummary(ChannelSummaryResponse dto)
        {
            if (dto == null)
            {
                return null;
            }

            var lastMessage = DtoToChatMessage(dto.LastMessage);
            var member = dto.Members.First();
            var directMember = dto.Members.FirstOrDefault(x => x.Id != member.Id);

            return new ChatSummaryModel
            {
                Id = dto.Id.ToString(),
                Name = dto.Name,
                PhotoUrl = dto.PhotoUrl,
                LastMessage = lastMessage,
                IsMuted = dto.IsMuted,
                CreatedDate = dto.Created,
                UpdatedDate = dto.Updated,
                UnreadMessagesCount = dto.UnreadMessagesCount,
                Type = (Models.ChannelType) dto.Type,
                Member = DtoToChatUser(member),
                DirectMember = DtoToChatUser(directMember)
            };
        }

        internal static ChatMessageModel DtoToChatMessage(MessageResponse dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new ChatMessageModel
            {
                Id = dto.Id.ToString(),
                Body = dto.Body,
                ChannelId = dto.ChannelId.ToString(),
                DateTime = dto.Created,
                SenderId = dto.Sender?.Id.ToString(),
                SenderName = dto.Sender?.UserName,
                SenderPhotoUrl = dto.Sender?.AvatarUrl,
                MessageType = DtoToMessageType(dto.Type),
                IsRead = dto.IsRead,
                IsDelivered = true,
                ImageRemoteUrl = dto.ImageUrl,
                ChannelType = (Models.ChannelType) dto.ChannelType
            };
        }

        internal static ChatUserModel DtoToChatUser(MemberSummaryResponse dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new ChatUserModel
            {
                Id = dto.Id.ToString(),
                Username = dto.UserName,
                PhotoUrl = dto.AvatarUrl,
                LastActivity = dto.LastActivity,
                IsOnline = dto.Status == UserStatus.Online,
                Role = DtoToUserRole(dto.Role),
                IsActive = dto.IsActive
            };
        }

        private static Models.MessageType DtoToMessageType(MessageType dto)
        {
            switch (dto)
            {
                case MessageType.Default:
                    return Models.MessageType.Default;
                case MessageType.System:
                    return Models.MessageType.System;
                default: throw new InvalidEnumArgumentException();
            }
        }

        private static Models.Enum.UserRole DtoToUserRole(UserRole dto)
        {
            switch (dto)
            {
                case UserRole.User:
                    return Models.Enum.UserRole.User;
                case UserRole.Admin:
                    return Models.Enum.UserRole.Admin;
                default: throw new InvalidEnumArgumentException();
            }
        }

        #endregion
    }
}
