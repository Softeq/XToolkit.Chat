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
	[Register ("CreateChatViewController")]
	partial class CreateChatViewController
	{
		[Outlet]
		UIKit.UINavigationBar CustomNavigationBar { get; set; }

		[Outlet]
		UIKit.UINavigationItem CustomNavigationItem { get; set; }

		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UITableView TableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CustomNavigationBar != null) {
				CustomNavigationBar.Dispose ();
				CustomNavigationBar = null;
			}

			if (CustomNavigationItem != null) {
				CustomNavigationItem.Dispose ();
				CustomNavigationItem = null;
			}

			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
			}
		}
	}
}
