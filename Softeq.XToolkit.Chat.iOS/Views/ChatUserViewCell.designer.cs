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
	[Register ("ChatUserViewCell")]
	partial class ChatUserViewCell
	{
		[Outlet]
		UIKit.UIButton CheckBoxButton { get; set; }

		[Outlet]
		FFImageLoading.Cross.MvxCachedImageView PhotoImageView { get; set; }

		[Outlet]
		UIKit.UILabel UsernameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CheckBoxButton != null) {
				CheckBoxButton.Dispose ();
				CheckBoxButton = null;
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
