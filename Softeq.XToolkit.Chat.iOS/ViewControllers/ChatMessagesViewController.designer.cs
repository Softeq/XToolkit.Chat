// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    [Register ("ChatMessagesViewController")]
    partial class ChatMessagesViewController
    {
        [Outlet]
        UIKit.UINavigationBar CustomNavigationBar { get; set; }


        [Outlet]
        UIKit.UINavigationItem CustomNavigationItem { get; set; }


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
            if (CustomNavigationBar != null) {
                CustomNavigationBar.Dispose ();
                CustomNavigationBar = null;
            }

            if (CustomNavigationItem != null) {
                CustomNavigationItem.Dispose ();
                CustomNavigationItem = null;
            }

            if (MainView != null) {
                MainView.Dispose ();
                MainView = null;
            }

            if (MainViewBottomConstraint != null) {
                MainViewBottomConstraint.Dispose ();
                MainViewBottomConstraint = null;
            }

            if (ScrollToBottomButton != null) {
                ScrollToBottomButton.Dispose ();
                ScrollToBottomButton = null;
            }
        }
    }
}