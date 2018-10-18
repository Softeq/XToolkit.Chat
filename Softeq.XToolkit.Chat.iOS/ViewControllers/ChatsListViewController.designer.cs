// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
	[Register ("ChatsListViewController")]
	partial class ChatsListViewController
	{
		[Outlet]
		UIKit.UITableView ChatsTableView { get; set; }

		[Outlet]
		Softeq.XToolkit.Chat.iOS.Views.ConnectionStatusView CustomTitleView { get; set; }

		[Outlet]
		UIKit.UIButton LoginButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ChatsTableView != null) {
				ChatsTableView.Dispose ();
				ChatsTableView = null;
			}

			if (CustomTitleView != null) {
				CustomTitleView.Dispose ();
				CustomTitleView = null;
			}

			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}
		}
	}
}
