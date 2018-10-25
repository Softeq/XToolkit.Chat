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
		UIKit.UINavigationBar CustomNavigationBar { get; set; }

		[Outlet]
		UIKit.UINavigationItem CustomNavigationItem { get; set; }

		[Outlet]
		UIKit.UIButton LoginButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ChatsTableView != null) {
				ChatsTableView.Dispose ();
				ChatsTableView = null;
			}

			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}

			if (CustomNavigationItem != null) {
				CustomNavigationItem.Dispose ();
				CustomNavigationItem = null;
			}

			if (CustomNavigationBar != null) {
				CustomNavigationBar.Dispose ();
				CustomNavigationBar = null;
			}
		}
	}
}
