// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Drawing;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS
{
    /// <summary>
    /// Shared style configurations of chat.
    /// </summary>
    public static class Styles
    {
        public static UIColor NavigationBarTintColor { get; set; } = UIColor.FromRGB(62, 218, 215);
        public static string BackButtonBundleName { get; set; }
        public static string ChatDetailsButtonBundleName { get; set; }
        public static string ChatGroupNoAvatarBundleName { get; set; }

        public static MvxCachedImageViewExtensions.AvatarStyles AvatarStyles { get; set; }
    }
}
