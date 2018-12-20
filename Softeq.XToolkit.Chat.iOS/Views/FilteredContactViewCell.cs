using System;
using System.Collections.Generic;
using FFImageLoading.Transformations;
using Foundation;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.Extensions;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using UIKit;
using Softeq.XToolkit.Common.iOS.Extensions;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class FilteredContactViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(FilteredContactViewCell));
        public static readonly UINib Nib;

        private readonly List<Binding> _bindings = new List<Binding>();

        private WeakReferenceEx<ChatUserViewModel> _viewModelRef;
        private UITapGestureRecognizer _tapGesture;

        static FilteredContactViewCell()
        {
            Nib = UINib.FromName(nameof(FilteredContactViewCell), NSBundle.MainBundle);
        }

        protected FilteredContactViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            Initialize();
        }

        private void Initialize()
        {
            OnlineIndicatorView.BackgroundColor = StyleHelper.Style.OnlineStatusColor;
            OnlineIndicatorView.WithBorder(1f).AsCircle();
        }

        public void BindViewModel(ChatUserViewModel viewModel)
        {
            _viewModelRef = WeakReferenceEx.Create(viewModel);

            _bindings.DetachAllAndClear();

            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.Username, () => UsernameLabel.Text));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.PhotoUrl).WhenSourceChanges(() =>
            {
                PhotoImageView.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.PhotoUrl,
                    _viewModelRef.Target.Username,
                    StyleHelper.Style.AvatarStyles,
                    x => x.Transform(new CircleTransformation()));
            }));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsSelected, () => CheckBoxButton.Selected, BindingMode.TwoWay));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsActive).WhenSourceChanges(() =>
            {
                var isActive = _viewModelRef.Target.IsActive;

                InactiveContactOverlay.Hidden = isActive;
                CheckBoxButton.Hidden = !isActive;
                UserInteractionEnabled = isActive;
            }));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsOnline, () => OnlineIndicatorView.Hidden)
                .ConvertSourceToTarget(x => !x));

            _tapGesture = new UITapGestureRecognizer(() =>
            {
                _viewModelRef.Target.IsSelected = !_viewModelRef.Target.IsSelected;
            })
            {
                NumberOfTapsRequired = 1
            };
            AddGestureRecognizer(_tapGesture);
        }

        protected override void Dispose(bool disposing)
        {
            RemoveGestureRecognizer(_tapGesture);

            base.Dispose(disposing);
        }
    }
}
