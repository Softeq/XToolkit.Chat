using System;
using UIKit;
using CoreGraphics;
using System.Windows.Input;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class NewGroupView : CustomViewBase
    {
        protected NewGroupView(IntPtr handle) : base(handle)
        {
        }

        public NewGroupView(CGRect frame) : base(frame)
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            ImageView.Image = UIImage.FromBundle(StyleHelper.Style.ChatNewGroupAvatarBundleName);
            TitleView.TextColor = StyleHelper.Style.AccentColor;
        }

        public void SetText(string text)
        {
            TitleView.Text = text;
        }

        public void SetCommand(ICommand command)
        {
            NewGroupButton.SetCommand(command);
        }
    }
}
