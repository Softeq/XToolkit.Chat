using System;
using FFImageLoading.Transformations;
using Foundation;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using UIKit;
using Softeq.XToolkit.Common.iOS.Extensions;
using Softeq.XToolkit.Chat.iOS.Controls;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class FilteredContactViewCell : BindableTableViewCell<ChatUserViewModel>
    {
        public static readonly string Key = nameof(FilteredContactViewCell);
        public static readonly UINib Nib;

        static FilteredContactViewCell()
        {
            Nib = UINib.FromName(Key, NSBundle.MainBundle);
        }

        protected FilteredContactViewCell(IntPtr handle) : base(handle) { }

        protected override void Initialize()
        {
            OnlineIndicatorView.BackgroundColor = StyleHelper.Style.OnlineStatusColor;
            OnlineIndicatorView.WithBorder(1f).AsCircle();
        }

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
            Bindings.Add(this.SetBinding(() => ViewModel.Target.IsActive).WhenSourceChanges(() =>
            {
                var isActive = ViewModel.Target.IsActive;

                InactiveContactOverlay.Hidden = isActive;
                CheckBoxButton.Hidden = !isActive;
                UserInteractionEnabled = isActive;
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.Target.IsOnline, () => OnlineIndicatorView.Hidden)
                .ConvertSourceToTarget(x => !x));
        }
    }
}
