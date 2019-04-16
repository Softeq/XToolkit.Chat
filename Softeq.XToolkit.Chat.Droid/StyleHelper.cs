// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;

namespace Softeq.XToolkit.Chat.Droid
{
    internal static class StyleHelper
    {
        private static readonly Lazy<IChatDroidStyle> StyleLazy = Dependencies.IocContainer.LazyResolve<IChatDroidStyle>();

        public static IChatDroidStyle Style => StyleLazy.Value;
    }

    public interface IChatDroidStyle
    {
        AvatarPlaceholderDrawable.AvatarStyles ChatAvatarStyles { get; }

        int NavigationBarBackButtonIcon { get; }
        int NavigationBarDetailsButtonIcon { get; }
        int TakeAttachmentButtonIcon { get; }
        int AddAttachmentButtonIcon { get; }
        int SendMessageButtonIcon { get; }
        int EditingCloseButtonIcon { get; }
        int ScrollDownButtonIcon { get; }
        int RemoveImageButtonIcon { get; }
        int MessageStatusSentIcon { get; }
        int MessageStatusDeliveredIcon { get; }
        int MessageStatusReadIcon { get; }
        int CommonActivityStyle { get; }
        int ChatGroupNoAvatarIcon { get; }
        int IncomingMessageBg { get; }
        int OutcomingMessageBg { get; }
        int ContentColor { get; }
        int UnderlinedBg { get; }
        int CheckedIcon { get; }
        int UnCheckedIcon { get; }
        int UnreadMessagesCountColor { get; }
        int UnreadMutedMessagesCountColor { get; }
        bool UseLogoInsteadOfConnectionStatus { get; }
        int LogoIcon { get; }
        int NewGroupAvatarIcon { get; }
        int AddMemberIcon { get; }
        int AttachmentImagePlaceholder { get; }
    }

    public abstract class ChatDroidStyleBase : IChatDroidStyle
    {
        public int CommonActivityStyle { get; } = Resource.Style.ChatTheme;
        public int EditingCloseButtonIcon { get; } = Resource.Drawable.chat_ic_close;
        public int ScrollDownButtonIcon { get; } = Resource.Drawable.chat_conversation_scroll_down;
        public int MessageStatusSentIcon { get; } = Resource.Drawable.chat_ic_sent;
        public int MessageStatusDeliveredIcon { get; } = Resource.Drawable.chat_ic_delivered;
        public int MessageStatusReadIcon { get; } = Resource.Drawable.chat_ic_read;

        public abstract int NavigationBarBackButtonIcon { get; }
        public abstract int NavigationBarDetailsButtonIcon { get; }
        public abstract int AddAttachmentButtonIcon { get; }
        public abstract int TakeAttachmentButtonIcon { get; }
        public abstract int SendMessageButtonIcon { get; }
        public abstract AvatarPlaceholderDrawable.AvatarStyles ChatAvatarStyles { get; }
        public abstract int ChatGroupNoAvatarIcon { get; }
        public abstract int RemoveImageButtonIcon { get; }
        public abstract int IncomingMessageBg { get; }
        public abstract int OutcomingMessageBg { get; }
        public abstract int ContentColor { get; }
        public abstract int UnderlinedBg { get; }
        public abstract int CheckedIcon { get; }
        public abstract int UnCheckedIcon { get; }
        public abstract int UnreadMessagesCountColor { get; }
        public abstract int UnreadMutedMessagesCountColor { get; }
        public abstract bool UseLogoInsteadOfConnectionStatus { get; }
        public abstract int LogoIcon { get; }
        public abstract int NewGroupAvatarIcon { get; }
        public abstract int AddMemberIcon { get; }
        public abstract int AttachmentImagePlaceholder { get; }
    }
}