// Developed by Softeq Development Corporation
// http://www.softeq.com

using UIKit;
using static Softeq.XToolkit.WhiteLabel.iOS.Shared.AvatarImageHelpers;

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

        public static AvatarStyles AvatarStyles { get; set; }
    }
}
