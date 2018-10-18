// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    [Register ("ChatUserViewCell")]
    partial class ChatUserViewCell
    {
        [Outlet]
        UIKit.UISwitch IsSelectedSwitch { get; set; }


        [Outlet]
        FFImageLoading.Cross.MvxCachedImageView PhotoImageView { get; set; }


        [Outlet]
        UIKit.UILabel UsernameLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (IsSelectedSwitch != null) {
                IsSelectedSwitch.Dispose ();
                IsSelectedSwitch = null;
            }

            if (PhotoImageView != null) {
                PhotoImageView.Dispose ();
                PhotoImageView = null;
            }

            if (UsernameLabel != null) {
                UsernameLabel.Dispose ();
                UsernameLabel = null;
            }
        }
    }
}