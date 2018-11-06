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
	[Register ("ChatDetailsHeaderView")]
	partial class ChatDetailsHeaderView
	{
		[Outlet]
		UIKit.UIButton AddMembersButton { get; set; }

		[Outlet]
		UIKit.UIButton ChangeChatPhotoButton { get; set; }

		[Outlet]
		UIKit.UIView ChatAvatarcontainer { get; set; }

		[Outlet]
		FFImageLoading.Cross.MvxCachedImageView ChatAvatarImageView { get; set; }

		[Outlet]
		UIKit.UITextField ChatNameTextField { get; set; }

		[Outlet]
		FFImageLoading.Cross.MvxCachedImageView EditedChatAvatarImageView { get; set; }

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

			if (EditedChatAvatarImageView != null) {
				EditedChatAvatarImageView.Dispose ();
				EditedChatAvatarImageView = null;
			}

			if (MembersCountLabel != null) {
				MembersCountLabel.Dispose ();
				MembersCountLabel = null;
			}

			if (ChatAvatarcontainer != null) {
				ChatAvatarcontainer.Dispose ();
				ChatAvatarcontainer = null;
			}
		}
	}
}
