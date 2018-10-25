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
	[Register ("SelectContactsViewController")]
	partial class SelectContactsViewController
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
			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
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
