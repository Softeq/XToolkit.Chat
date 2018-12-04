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
	[Register ("SelectedMemberViewCell")]
	partial class SelectedMemberViewCell
	{
		[Outlet]
		UIKit.UILabel MemberNameLabel { get; set; }

		[Outlet]
		UIKit.UIImageView MemberPhotoImageView { get; set; }

		[Outlet]
		UIKit.UIButton RemoveMemberBtn { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (MemberPhotoImageView != null) {
				MemberPhotoImageView.Dispose ();
				MemberPhotoImageView = null;
			}

			if (RemoveMemberBtn != null) {
				RemoveMemberBtn.Dispose ();
				RemoveMemberBtn = null;
			}

			if (MemberNameLabel != null) {
				MemberNameLabel.Dispose ();
				MemberNameLabel = null;
			}
		}
	}
}
