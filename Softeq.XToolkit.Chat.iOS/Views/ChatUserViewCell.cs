// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using FFImageLoading.Transformations;
using Foundation;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.iOS.Controls;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ChatUserViewCell : BindableTableViewCell<ChatUserViewModel>
    {
        public static readonly string Key = nameof(ChatUserViewCell);
        public static readonly UINib Nib;

        static ChatUserViewCell()
        {
            Nib = UINib.FromName(Key, NSBundle.MainBundle);
        }

        protected ChatUserViewCell(IntPtr handle) : base(handle) { }

        protected override void DoAttachBindings()
        {
            Bindings.Add(this.SetBinding(() => ViewModel.Target.Username, () => UsernameLabel.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.Target.PhotoUrl).WhenSourceChanges(() =>
            {
                PhotoImageView.LoadImageWithTextPlaceholder(
                    ViewModel.Target.PhotoUrl,
                    ViewModel.Target.Username,
                    StyleHelper.Style.AvatarStyles,
                    x => x.Transform(new CircleTransformation()));
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.Target.IsSelected, () => CheckBoxButton.Selected, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.Target.IsSelectable, () => CheckBoxButton.Hidden)
                .ConvertSourceToTarget(x => !x));
        }
    }
}
