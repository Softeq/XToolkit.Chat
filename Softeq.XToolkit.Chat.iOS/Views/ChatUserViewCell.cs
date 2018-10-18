// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Foundation;
using UIKit;
using System.Collections.Generic;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ChatUserViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ChatUserViewCell));
        public static readonly UINib Nib;

        List<Binding> _bindings = new List<Binding>();

        private WeakReferenceEx<ChatUserViewModel> _viewModelRef;

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

            _bindings.Apply(x => x.Detach());
            _bindings.Clear();

            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.Username, () => UsernameLabel.Text));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.PhotoUrl).WhenSourceChanges(() =>
            {
                PhotoImageView.LoadImageAsync("NoPhoto", _viewModelRef.Target.PhotoUrl);
            }));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsSelected, () => IsSelectedSwitch.On, BindingMode.TwoWay));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsSelectable, () => IsSelectedSwitch.Hidden)
                          .ConvertSourceToTarget(x => !x));
        }
    }
}
