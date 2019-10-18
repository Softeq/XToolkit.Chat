// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.ComponentModel;
using System.Linq;
using Softeq.NetKit.Chat.TransportModels.Enums;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Channel;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Member;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Message;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.WhiteLabel.Model;
using MessageType = Softeq.NetKit.Chat.TransportModels.Enums.MessageType;

namespace Softeq.XToolkit.Chat.HttpClient
{
    internal static class Mapper
    {
        #region Common Mapper Methods - SignalRClient.Mapper

        internal static ChatSummaryModel DtoToChatSummary(ChannelSummaryResponse response)
        {
            if (response == null)
            {
                return null;
            }
            var lastMessage = DtoToChatMessage(response.LastMessage);
            var member = response.Members.First();
            var directMember = response.Members.FirstOrDefault(x => x.Id != member.Id);

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
                Type = (Models.ChannelType) response.Type,
                Member = DtoToChatUser(member),
                DirectMember = DtoToChatUser(directMember)
            };
        }

        internal static ChatMessageModel DtoToChatMessage(MessageResponse response)
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
                ChannelType = (Models.ChannelType) response.ChannelType
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

        internal static PagingModel<TModel> PagedDtoToPagingModel<TDto, TModel>(
            QueryResult<TDto> dto,
            Func<TDto, TModel> itemsMapper)
        {
            if (dto?.Results == null)
            {
                return null;
            }

            var items = dto.Results
                .Select(itemsMapper)
                .ToList();

            return new PagingModel<TModel>
            {
                Page = dto.PageNumber,
                Data = items,
                TotalNumberOfPages = dto.TotalNumberOfPages,
                TotalNumberOfRecords = dto.TotalNumberOfItems,
                PageSize = dto.PageSize
            };
        }
    }
}
