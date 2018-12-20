﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

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

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ChatUserViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ChatUserViewCell));
        public static readonly UINib Nib;

        private readonly List<Binding> _bindings = new List<Binding>();

        private WeakReferenceEx<ChatUserViewModel> _viewModelRef;
        private UITapGestureRecognizer _tapGesture;

        static ChatUserViewCell()
        {
            Nib = UINib.FromName(nameof(ChatUserViewCell), NSBundle.MainBundle);
        }

        protected ChatUserViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
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
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsSelectable, () => CheckBoxButton.Hidden)
                .ConvertSourceToTarget(x => !x));

            _tapGesture = new UITapGestureRecognizer(() =>
            {
                _viewModelRef.Target.IsSelected = !_viewModelRef.Target.IsSelected;
            })
            {
                NumberOfTapsRequired = 1
            };

            UserInteractionEnabled = true;
            AddGestureRecognizer(_tapGesture);
        }

        protected override void Dispose(bool disposing)
        {
            RemoveGestureRecognizer(_tapGesture);

            base.Dispose(disposing);
        }
    }
}
