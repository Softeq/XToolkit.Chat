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
	[Register ("AddContactsViewController")]
	partial class AddContactsViewController
	{
		[Outlet]
		UIKit.UINavigationBar CustomNavigationBar { get; set; }

		[Outlet]
		UIKit.UINavigationItem CustomNavigationBarItem { get; set; }

		[Outlet]
		UIKit.UICollectionView SelectedMembersCollectionView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint SelectedMembersCollectionViewTopConstraint { get; set; }

		[Outlet]
		UIKit.UITableView TableView { get; set; }

		[Outlet]
		UIKit.UISearchBar TableViewSearchBar { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CustomNavigationBar != null) {
				CustomNavigationBar.Dispose ();
				CustomNavigationBar = null;
			}

			if (CustomNavigationBarItem != null) {
				CustomNavigationBarItem.Dispose ();
				CustomNavigationBarItem = null;
			}

			if (SelectedMembersCollectionView != null) {
				SelectedMembersCollectionView.Dispose ();
				SelectedMembersCollectionView = null;
			}

			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
			}

			if (TableViewSearchBar != null) {
				TableViewSearchBar.Dispose ();
				TableViewSearchBar = null;
			}

			if (SelectedMembersCollectionViewTopConstraint != null) {
				SelectedMembersCollectionViewTopConstraint.Dispose ();
				SelectedMembersCollectionViewTopConstraint = null;
			}
		}
	}
}
