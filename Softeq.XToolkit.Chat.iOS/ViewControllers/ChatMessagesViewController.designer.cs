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
	[Register ("ChatMessagesViewController")]
	partial class ChatMessagesViewController
	{
		[Outlet]
		Softeq.XToolkit.Chat.iOS.Views.ConnectionStatusView CustomTitleView { get; set; }

		[Outlet]
		UIKit.UIView MainView { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint MainViewBottomConstraint { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint ScrollDownBottomConstraint { get; set; }

		[Outlet]
		UIKit.UIButton ScrollToBottomButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CustomTitleView != null) {
				CustomTitleView.Dispose ();
				CustomTitleView = null;
			}

			if (MainView != null) {
				MainView.Dispose ();
				MainView = null;
			}

			if (MainViewBottomConstraint != null) {
				MainViewBottomConstraint.Dispose ();
				MainViewBottomConstraint = null;
			}

			if (ScrollDownBottomConstraint != null) {
				ScrollDownBottomConstraint.Dispose ();
				ScrollDownBottomConstraint = null;
			}

			if (ScrollToBottomButton != null) {
				ScrollToBottomButton.Dispose ();
				ScrollToBottomButton = null;
			}
		}
	}
}
