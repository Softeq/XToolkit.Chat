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
	[Register ("FilteredContactViewCell")]
	partial class FilteredContactViewCell
	{
		[Outlet]
		Softeq.XToolkit.Chat.iOS.Views.CheckBoxView CheckBoxButton { get; set; }

		[Outlet]
		UIKit.UIView InactiveContactOverlay { get; set; }

		[Outlet]
		UIKit.UIView OnlineIndicatorView { get; set; }

		[Outlet]
		FFImageLoading.Cross.MvxCachedImageView PhotoImageView { get; set; }

		[Outlet]
		UIKit.UILabel UsernameLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (UsernameLabel != null) {
				UsernameLabel.Dispose ();
				UsernameLabel = null;
			}

			if (CheckBoxButton != null) {
				CheckBoxButton.Dispose ();
				CheckBoxButton = null;
			}

			if (PhotoImageView != null) {
				PhotoImageView.Dispose ();
				PhotoImageView = null;
			}

			if (OnlineIndicatorView != null) {
				OnlineIndicatorView.Dispose ();
				OnlineIndicatorView = null;
			}

			if (InactiveContactOverlay != null) {
				InactiveContactOverlay.Dispose ();
				InactiveContactOverlay = null;
			}
		}
	}
}
