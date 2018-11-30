// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Windows.Input;
using FFImageLoading.Transformations;
using Foundation;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class SelectedMemberViewCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString(nameof(SelectedMemberViewCell));
        public static readonly UINib Nib;

        protected SelectedMemberViewCell(IntPtr handle) : base(handle) { }

        static SelectedMemberViewCell()
        {
            Nib = UINib.FromName(Key, null);
        }

        public void Bind(ChatUserViewModel memberViewModel, ICommand removeCommand)
        {
            //RemoveMemberBtn.SetCommand((RelayCommand<ChatUserViewModel>)removeCommand);

            //MemberPhotoImageView.LoadImageWithTextPlaceholder(
            //memberViewModel.PhotoUrl,
            //memberViewModel.Username,
            //StyleHelper.Style.AvatarStyles,
            //x => x.Transform(new CircleTransformation()));
        }
    }
}
