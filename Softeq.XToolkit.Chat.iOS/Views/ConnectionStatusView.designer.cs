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
    [Register("ConnectionStatusView")]
    partial class ConnectionStatusView
	{
		[Outlet]
		UIKit.UIActivityIndicatorView Spinner { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (Spinner != null) {
				Spinner.Dispose ();
				Spinner = null;
			}
		}
	}
}
