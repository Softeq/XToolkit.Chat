// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Foundation;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class MessagesDateHeaderViewCell : UITableViewHeaderFooterView
    {
        public static readonly NSString Key = new NSString(nameof(MessagesDateHeaderViewCell));
        public static readonly UINib Nib;

        static MessagesDateHeaderViewCell()
        {
            Nib = UINib.FromName(nameof(MessagesDateHeaderViewCell), NSBundle.MainBundle);
        }

        protected MessagesDateHeaderViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public string Title
        {
            set => TitleLabel.Text = value;
        }
    }
}
