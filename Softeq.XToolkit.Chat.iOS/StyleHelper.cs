﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.iOS.Shared;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS
{
    internal static class StyleHelper
    {
        private static readonly Lazy<IChatIosStyle> StyleLazy = ServiceLocator.LazyResolve<IChatIosStyle>();

        public static IChatIosStyle Style => StyleLazy.Value;
    }

    public interface IChatIosStyle
    {
        UIColor NavigationBarTintColor { get; }
        UIColor ButtonTintColor { get; }

        string BackButtonBundleName { get; }
        string ChatDetailsButtonBundleName { get; }
        string ChatGroupNoAvatarBundleName { get; }
        string TakePhotoBundleName { get; }
        string SendBundleName { get; }
        string AddImageBundleName { get; }
        string RemoveAttachBundleName { get; }

        AvatarImageHelpers.AvatarStyles AvatarStyles { get; }
    }
}
