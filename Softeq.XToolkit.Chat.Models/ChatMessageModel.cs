// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Common.Extensions;

namespace Softeq.XToolkit.Chat.Models
{
    public class ChatMessageModel : IEquatable<ChatMessageModel>
    {
        public string Id { get; set; }
        public string ChannelId { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderPhotoUrl { get; set; }
        public bool IsMine { get; set; }
        public bool IsDelivered { get; set; }
        public bool IsRead { get; set; }
        public MessageType MessageType { get; set; }
        public string Body { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public string ImageCacheKey { get; set; }
        public string ImageRemoteUrl { get; set; }
        public ChannelType ChannelType { get; set; }

        public ChatMessageStatus Status
        {
            get
            {
                if (!IsMine)
                {
                    return ChatMessageStatus.Other;
                }
                if (!IsDelivered)
                {
                    return ChatMessageStatus.Sending;
                }
                return IsRead ? ChatMessageStatus.Read : ChatMessageStatus.Delivered;
            }
        }

        public void UpdateIsMineStatus(string currentUserId)
        {
            IsMine = SenderId == currentUserId;
        }

        public void UpdateMessage(ChatMessageModel chatMessageModel)
        {
            if (chatMessageModel == null)
            {
                return;
            }
            Id = chatMessageModel.Id;
            ChannelId = chatMessageModel.ChannelId;
            SenderId = chatMessageModel.SenderId;
            SenderName = chatMessageModel.SenderName;
            SenderPhotoUrl = chatMessageModel.SenderPhotoUrl;
            IsMine = chatMessageModel.IsMine;
            IsDelivered = chatMessageModel.IsDelivered;
            IsRead = chatMessageModel.IsRead;
            MessageType = chatMessageModel.MessageType;
            Body = chatMessageModel.Body;
            DateTime = chatMessageModel.DateTime;
            ImageRemoteUrl = chatMessageModel.ImageRemoteUrl;
            ChannelType = chatMessageModel.ChannelType;
        }

        public bool IsEarlierThan(ChatMessageModel message)
        {
            return message != null && DateTime.DateTime.IsEarlierThan(message.DateTime.DateTime);
        }

        public bool IsEarlierOrEqualsThan(ChatMessageModel message)
        {
            var result = message != null && DateTime.DateTime.IsEarlierOrEqualThan(message.DateTime.DateTime);
            return message != null && DateTime.DateTime.IsEarlierOrEqualThan(message.DateTime.DateTime);
        }

        public bool IsLaterThan(ChatMessageModel message)
        {
            return message != null && DateTime.DateTime.IsLaterThan(message.DateTime.DateTime);
        }

        public bool IsLaterOrEqualsThan(ChatMessageModel message)
        {
            return message != null && DateTime.DateTime.IsLaterOrEqualThan(message.DateTime.DateTime);
        }

        public bool Equals(ChatMessageModel other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            if (Id != null)
            {
                return Id == other.Id;
            }
            return Body == other.Body && DateTime == other.DateTime;
        }

        public override bool Equals(object obj) => Equals(obj as ChatMessageModel);

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }
    }
}
