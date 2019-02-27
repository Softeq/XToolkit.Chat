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
	[Register ("NewGroupView")]
	partial class NewGroupView
	{
		[Outlet]
		UIKit.UIImageView ImageView { get; set; }

		[Outlet]
		UIKit.UIButton NewGroupButton { get; set; }

		[Outlet]
		UIKit.UILabel TitleView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ImageView != null) {
				ImageView.Dispose ();
				ImageView = null;
			}

			if (NewGroupButton != null) {
				NewGroupButton.Dispose ();
				NewGroupButton = null;
			}

			if (TitleView != null) {
				TitleView.Dispose ();
				TitleView = null;
			}
		}
	}
}
