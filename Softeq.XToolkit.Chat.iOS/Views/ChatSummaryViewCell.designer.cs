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
	[Register ("ChatSummaryViewCell")]
	partial class ChatSummaryViewCell
	{
		[Outlet]
		UIKit.UILabel ChatNameLabel { get; set; }

		[Outlet]
		UIKit.UILabel DateTimeLabel { get; set; }

		[Outlet]
		UIKit.UIImageView LastMessageBodyPhotoIcon { get; set; }

		[Outlet]
		UIKit.UILabel LastMessageBodyPhotoLabel { get; set; }

		[Outlet]
		UIKit.UIView LastMessageBodyPhotoView { get; set; }

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

			if (LastMessageBodyPhotoView != null) {
				LastMessageBodyPhotoView.Dispose ();
				LastMessageBodyPhotoView = null;
			}

			if (LastMessageBodyPhotoIcon != null) {
				LastMessageBodyPhotoIcon.Dispose ();
				LastMessageBodyPhotoIcon = null;
			}

			if (LastMessageBodyPhotoLabel != null) {
				LastMessageBodyPhotoLabel.Dispose ();
				LastMessageBodyPhotoLabel = null;
			}
		}
	}
}
