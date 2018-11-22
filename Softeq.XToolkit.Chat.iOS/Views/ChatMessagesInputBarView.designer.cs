// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Softeq.XToolkit.Chat.iOS.Views
{
	[Register ("ChatMessagesInputBarView")]
	partial class ChatMessagesInputBarView
	{
		[Outlet]
		FFImageLoading.Cross.MvxCachedImageView AttachedImageView { get; set; }

		[Outlet]
		UIKit.UIButton AttachImageButton { get; set; }

		[Outlet]
		UIKit.UIView EditImageContainer { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint EditImageContainerHeightConstraint { get; set; }

		[Outlet]
		UIKit.UIButton EditingCloseButton { get; set; }

		[Outlet]
		UIKit.UIView EditingIndicatorView { get; set; }

		[Outlet]
		UIKit.UILabel EditingText { get; set; }

		[Outlet]
		UIKit.UIView EditViewContainer { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint EditViewContainerHeightConstraint { get; set; }

		[Outlet]
		UIKit.UITextView InputTextView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint InputTextViewMaxHeightConstraint { get; set; }

		[Outlet]
		UIKit.UILabel InputTextViewPlaceholder { get; set; }

		[Outlet]
		UIKit.UIView InputViewContainer { get; set; }

		[Outlet]
		UIKit.UIButton RemoveAttachButton { get; set; }

		[Outlet]
		UIKit.UIButton SendButton { get; set; }

		[Outlet]
		UIKit.UIButton TakePhotoButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (AttachedImageView != null) {
				AttachedImageView.Dispose ();
				AttachedImageView = null;
			}

			if (AttachImageButton != null) {
				AttachImageButton.Dispose ();
				AttachImageButton = null;
			}

			if (EditImageContainer != null) {
				EditImageContainer.Dispose ();
				EditImageContainer = null;
			}

			if (EditImageContainerHeightConstraint != null) {
				EditImageContainerHeightConstraint.Dispose ();
				EditImageContainerHeightConstraint = null;
			}

			if (EditingCloseButton != null) {
				EditingCloseButton.Dispose ();
				EditingCloseButton = null;
			}

			if (EditingIndicatorView != null) {
				EditingIndicatorView.Dispose ();
				EditingIndicatorView = null;
			}

			if (EditingText != null) {
				EditingText.Dispose ();
				EditingText = null;
			}

			if (EditViewContainer != null) {
				EditViewContainer.Dispose ();
				EditViewContainer = null;
			}

			if (EditViewContainerHeightConstraint != null) {
				EditViewContainerHeightConstraint.Dispose ();
				EditViewContainerHeightConstraint = null;
			}

			if (InputTextView != null) {
				InputTextView.Dispose ();
				InputTextView = null;
			}

			if (InputTextViewMaxHeightConstraint != null) {
				InputTextViewMaxHeightConstraint.Dispose ();
				InputTextViewMaxHeightConstraint = null;
			}

			if (InputTextViewPlaceholder != null) {
				InputTextViewPlaceholder.Dispose ();
				InputTextViewPlaceholder = null;
			}

			if (InputViewContainer != null) {
				InputViewContainer.Dispose ();
				InputViewContainer = null;
			}

			if (RemoveAttachButton != null) {
				RemoveAttachButton.Dispose ();
				RemoveAttachButton = null;
			}

			if (SendButton != null) {
				SendButton.Dispose ();
				SendButton = null;
			}

			if (TakePhotoButton != null) {
				TakePhotoButton.Dispose ();
				TakePhotoButton = null;
			}
		}
	}
}
