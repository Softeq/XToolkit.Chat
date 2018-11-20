using Softeq.XToolkit.WhiteLabel.Droid.Shared;

namespace Softeq.XToolkit.Chat.Droid
{
    public static class ExternalResourceIds
    {
        public static int NavigationBarBackButtonIcon { get; set; }
        public static int NavigationBarDetailsButtonIcon { get; set; }

        public static int AddAttachmentButtonIcon { get; set; }
        public static int SendMessageButtonIcon { get; set; }
        public static int EditingCloseButtonIcon { get; set; } = Resource.Drawable.chat_ic_close;
        public static int ScrollDownButtonIcon { get; set; } = Resource.Drawable.chat_conversation_scroll_down;

        public static int MessageStatusSentIcon { get; set; } = Resource.Drawable.chat_ic_sent;
        public static int MessageStatusDeliveredIcon { get; set; } = Resource.Drawable.chat_ic_delivered;
        public static int MessageStatusReadIcon { get; set; } = Resource.Drawable.chat_ic_read;

        public static AvatarPlaceholderDrawable.AvatarStyles ChatAvatarStyles { get; set; }
        public static int ChatGroupNoAvatarIcon { get; set; }
    }
}
