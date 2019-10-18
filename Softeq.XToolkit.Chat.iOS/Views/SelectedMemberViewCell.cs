// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using FFImageLoading.Transformations;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.iOS.Controls;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Commands;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class SelectedMemberViewCell : BindableCollectionViewCell<ChatUserViewModel>
    {
        public static readonly string Key = nameof(SelectedMemberViewCell);
        public static readonly UINib Nib;

        protected SelectedMemberViewCell(IntPtr handle) : base(handle) { }

        static SelectedMemberViewCell()
        {
            Nib = UINib.FromName(Key, null);
        }

        protected override void DoAttachBindings()
        {
            RemoveMemberBtn.SetCommand(new RelayCommand(RemoveSelection));
            RemoveMemberImage.Image = UIImage.FromBundle(StyleHelper.Style.RemoveAttachBundleName);

            MemberNameLabel.Text = ViewModel.Target.Username;

            MemberPhotoImageView.LoadImageWithTextPlaceholder(
                ViewModel.Target.PhotoUrl,
                ViewModel.Target.Username,
                StyleHelper.Style.AvatarStyles,
                x => x.Transform(new CircleTransformation()));
        }

        private void RemoveSelection()
        {
            ViewModel.Target.IsSelected = false;
        }
    }
}
