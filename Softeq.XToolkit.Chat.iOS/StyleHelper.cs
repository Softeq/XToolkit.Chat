// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.iOS.Helpers;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS
{
    internal static class StyleHelper
    {
        private static readonly Lazy<IChatIosStyle> StyleLazy = Dependencies.IocContainer.LazyResolve<IChatIosStyle>();

        public static IChatIosStyle Style => StyleLazy.Value;
    }

    public interface IChatIosStyle
    {
        UIColor AccentColor { get; }
        UIColor ButtonTintColor { get; }
        UIColor OnlineStatusColor { get; }

        AvatarImageHelpers.AvatarStyles AvatarStyles { get; }
        AvatarImageHelpers.AvatarStyles GroupDetailsAvatarStyles { get; }

        string BackButtonBundleName { get; }
        string ChatDetailsButtonBundleName { get; }
        string ChatGroupNoAvatarBundleName { get; }
        string TakePhotoBundleName { get; }
        string SendBundleName { get; }
        string AddImageBundleName { get; }
        string RemoveAttachBundleName { get; }
        string CloseButtonImageBoundleName { get; }
        string MessageSendingBoundleName { get; }
        string MessageDeliveredBoundleName { get; }
        string MessageReadBoundleName { get; }
        string BubbleMineBoundleName { get; }
        string BubbleOtherBoundleName { get; }
        string ScrollDownBoundleName { get; }
        string CheckedBoundleName { get; }
        string UnCheckedBoundleName { get; }
        bool UseLogoInsteadOfConnectionStatus { get; }
        string LogoBoundleName { get; }
        string ChatNewGroupAvatarBundleName { get; }
    }
}
