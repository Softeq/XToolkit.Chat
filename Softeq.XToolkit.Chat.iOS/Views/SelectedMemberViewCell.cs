// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using FFImageLoading.Transformations;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class SelectedMemberViewCell : UICollectionViewCell
    {
        public static readonly string Key = nameof(SelectedMemberViewCell);
        public static readonly UINib Nib;

        private ChatUserViewModel _memberViewModel;

        protected SelectedMemberViewCell(IntPtr handle) : base(handle) { }

        static SelectedMemberViewCell()
        {
            Nib = UINib.FromName(Key, null);
        }

        public void BindCell(ChatUserViewModel memberViewModel)
        {
            _memberViewModel = memberViewModel;

            RemoveMemberBtn.SetBackgroundImage(UIImage.FromBundle(StyleHelper.Style.RemoveAttachBundleName), UIControlState.Normal);
            RemoveMemberBtn.SetCommand(new RelayCommand(() =>
            {
                _memberViewModel.IsSelected = false;
            }));

            MemberNameLabel.Text = _memberViewModel.Username;

            MemberPhotoImageView.LoadImageWithTextPlaceholder(
                _memberViewModel.PhotoUrl,
                _memberViewModel.Username,
                StyleHelper.Style.AvatarStyles,
                x => x.Transform(new CircleTransformation()));
        }
    }
}
