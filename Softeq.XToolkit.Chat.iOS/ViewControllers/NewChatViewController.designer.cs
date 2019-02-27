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
	[Register ("NewChatViewController")]
	partial class NewChatViewController
	{
		[Outlet]
		UIKit.UINavigationBar CustomNavigationBar { get; set; }

		[Outlet]
		UIKit.UINavigationItem CustomNavigationBarItem { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView ProgressIndicator { get; set; }

		[Outlet]
		UIKit.UISearchBar SearchBar { get; set; }

		[Outlet]
		UIKit.UITableView TableView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CustomNavigationBarItem != null) {
				CustomNavigationBarItem.Dispose ();
				CustomNavigationBarItem = null;
			}

			if (ProgressIndicator != null) {
				ProgressIndicator.Dispose ();
				ProgressIndicator = null;
			}

			if (SearchBar != null) {
				SearchBar.Dispose ();
				SearchBar = null;
			}

			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
			}

			if (CustomNavigationBar != null) {
				CustomNavigationBar.Dispose ();
				CustomNavigationBar = null;
			}
		}
	}
}
