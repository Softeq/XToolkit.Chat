// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    [Register ("ChatInputView")]
    partial class ChatInputView
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView AttachmentContainer { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView AttachmentImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint BottomConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton DeleteButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView EditContainer { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel EditingBodyLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        public UIKit.UIButton EditingCloseButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel EditingTitleLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView ImageLoader { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton OpenCameraButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton OpenGalleryButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel PlaceholderLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton SendButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint TextMaxHeightConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        public Softeq.XToolkit.Chat.iOS.Views.ChatInputView.CustomTextView TextView { get; set; }

        [Action ("OnSend:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void OnSend (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (AttachmentContainer != null) {
                AttachmentContainer.Dispose ();
                AttachmentContainer = null;
            }

            if (AttachmentImage != null) {
                AttachmentImage.Dispose ();
                AttachmentImage = null;
            }

            if (BottomConstraint != null) {
                BottomConstraint.Dispose ();
                BottomConstraint = null;
            }

            if (DeleteButton != null) {
                DeleteButton.Dispose ();
                DeleteButton = null;
            }

            if (EditContainer != null) {
                EditContainer.Dispose ();
                EditContainer = null;
            }

            if (EditingBodyLabel != null) {
                EditingBodyLabel.Dispose ();
                EditingBodyLabel = null;
            }

            if (EditingCloseButton != null) {
                EditingCloseButton.Dispose ();
                EditingCloseButton = null;
            }

            if (EditingTitleLabel != null) {
                EditingTitleLabel.Dispose ();
                EditingTitleLabel = null;
            }

            if (ImageLoader != null) {
                ImageLoader.Dispose ();
                ImageLoader = null;
            }

            if (OpenCameraButton != null) {
                OpenCameraButton.Dispose ();
                OpenCameraButton = null;
            }

            if (OpenGalleryButton != null) {
                OpenGalleryButton.Dispose ();
                OpenGalleryButton = null;
            }

            if (PlaceholderLabel != null) {
                PlaceholderLabel.Dispose ();
                PlaceholderLabel = null;
            }

            if (SendButton != null) {
                SendButton.Dispose ();
                SendButton = null;
            }

            if (TextMaxHeightConstraint != null) {
                TextMaxHeightConstraint.Dispose ();
                TextMaxHeightConstraint = null;
            }

            if (TextView != null) {
                TextView.Dispose ();
                TextView = null;
            }
        }
    }
}