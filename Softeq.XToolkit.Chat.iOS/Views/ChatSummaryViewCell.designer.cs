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
    [Register ("ChatSummaryViewCell")]
    partial class ChatSummaryViewCell
    {
        [Outlet]
        UIKit.UILabel ChatNameLabel { get; set; }


        [Outlet]
        UIKit.UILabel DateTimeLabel { get; set; }


        [Outlet]
        UIKit.UILabel MessageBodyLabel { get; set; }


        [Outlet]
        UIKit.UIView ReadUnreadIndicator { get; set; }


        [Outlet]
        FFImageLoading.Cross.MvxCachedImageView SenderPhotoImageView { get; set; }


        [Outlet]
        UIKit.UIView UnreadMessageCountBackground { get; set; }


        [Outlet]
        UIKit.UILabel UnreadMessageCountLabel { get; set; }


        [Outlet]
        UIKit.UIView UnreadView { get; set; }


        [Outlet]
        UIKit.UILabel UsernameLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ChatNameLabel != null) {
                ChatNameLabel.Dispose ();
                ChatNameLabel = null;
            }

            if (DateTimeLabel != null) {
                DateTimeLabel.Dispose ();
                DateTimeLabel = null;
            }

            if (MessageBodyLabel != null) {
                MessageBodyLabel.Dispose ();
                MessageBodyLabel = null;
            }

            if (ReadUnreadIndicator != null) {
                ReadUnreadIndicator.Dispose ();
                ReadUnreadIndicator = null;
            }

            if (SenderPhotoImageView != null) {
                SenderPhotoImageView.Dispose ();
                SenderPhotoImageView = null;
            }

            if (UnreadMessageCountBackground != null) {
                UnreadMessageCountBackground.Dispose ();
                UnreadMessageCountBackground = null;
            }

            if (UnreadMessageCountLabel != null) {
                UnreadMessageCountLabel.Dispose ();
                UnreadMessageCountLabel = null;
            }

            if (UnreadView != null) {
                UnreadView.Dispose ();
                UnreadView = null;
            }

            if (UsernameLabel != null) {
                UsernameLabel.Dispose ();
                UsernameLabel = null;
            }
        }
    }
}