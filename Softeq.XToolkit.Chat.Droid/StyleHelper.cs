// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Droid.Shared;

namespace Softeq.XToolkit.Chat.Droid
{
    internal static class StyleHelper
    {
        private static readonly Lazy<IChatDroidStyle> StyleLazy = ServiceLocator.LazyResolve<IChatDroidStyle>();

        public static IChatDroidStyle Style => StyleLazy.Value;
    }

    public interface IChatDroidStyle
    {
        int NavigationBarBackButtonIcon { get; }
        int NavigationBarDetailsButtonIcon { get; }

        int AddAttachmentButtonIcon { get; }
        int SendMessageButtonIcon { get; }
        int EditingCloseButtonIcon { get; }
        int ScrollDownButtonIcon { get; }

        int MessageStatusSentIcon { get; }
        int MessageStatusDeliveredIcon { get; }
        int MessageStatusReadIcon { get; }

        AvatarPlaceholderDrawable.AvatarStyles ChatAvatarStyles { get; }
        int ChatGroupNoAvatarIcon { get; }
    }

    public abstract class ChatDroidStyleBase : IChatDroidStyle
    {
        public int EditingCloseButtonIcon { get; } = Resource.Drawable.chat_ic_close;
        public int ScrollDownButtonIcon { get; } = Resource.Drawable.chat_conversation_scroll_down;
        public int MessageStatusSentIcon { get; } = Resource.Drawable.chat_ic_sent;
        public int MessageStatusDeliveredIcon { get; } = Resource.Drawable.chat_ic_delivered;
        public int MessageStatusReadIcon { get; } = Resource.Drawable.chat_ic_read;

        public abstract int NavigationBarBackButtonIcon { get; }
        public abstract int NavigationBarDetailsButtonIcon { get; }
        public abstract int AddAttachmentButtonIcon { get; }
        public abstract int SendMessageButtonIcon { get; }
        public abstract AvatarPlaceholderDrawable.AvatarStyles ChatAvatarStyles { get; }
        public abstract int ChatGroupNoAvatarIcon { get; }
    }
}