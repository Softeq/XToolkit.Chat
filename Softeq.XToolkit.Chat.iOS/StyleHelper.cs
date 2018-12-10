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
        private static readonly Lazy<IChatIosStyle> StyleLazy = Dependencies.ServiceLocator.LazyResolve<IChatIosStyle>();

        public static IChatIosStyle Style => StyleLazy.Value;
    }

    public interface IChatIosStyle
    {
        UIColor NavigationBarTintColor { get; }
        UIColor ButtonTintColor { get; }

        AvatarImageHelpers.AvatarStyles AvatarStyles { get; }

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
    }
}
