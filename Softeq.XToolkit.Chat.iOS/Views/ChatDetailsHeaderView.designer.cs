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
    [Register ("ChatDetailsHeaderView")]
    partial class ChatDetailsHeaderView
    {
        [Outlet]
        UIKit.UIButton AddMembersButton { get; set; }


        [Outlet]
        UIKit.UIButton ChangeChatPhotoButton { get; set; }


        [Outlet]
        FFImageLoading.Cross.MvxCachedImageView ChatAvatarImageView { get; set; }


        [Outlet]
        UIKit.UITextField ChatNameTextField { get; set; }


        [Outlet]
        UIKit.UILabel MembersCountLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (AddMembersButton != null) {
                AddMembersButton.Dispose ();
                AddMembersButton = null;
            }

            if (ChangeChatPhotoButton != null) {
                ChangeChatPhotoButton.Dispose ();
                ChangeChatPhotoButton = null;
            }

            if (ChatAvatarImageView != null) {
                ChatAvatarImageView.Dispose ();
                ChatAvatarImageView = null;
            }

            if (ChatNameTextField != null) {
                ChatNameTextField.Dispose ();
                ChatNameTextField = null;
            }

            if (MembersCountLabel != null) {
                MembersCountLabel.Dispose ();
                MembersCountLabel = null;
            }
        }
    }
}