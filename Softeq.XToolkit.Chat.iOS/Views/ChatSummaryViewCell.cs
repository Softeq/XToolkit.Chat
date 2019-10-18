// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using Foundation;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Weak;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.iOS.Helpers;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ChatSummaryViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString(nameof(ChatSummaryViewCell));
        public static readonly UINib Nib;

        private List<Binding> _bindings = new List<Binding>();

        private WeakReferenceEx<ChatSummaryViewModel> _viewModelRef;

        static ChatSummaryViewCell()
        {
            Nib = UINib.FromName(nameof(ChatSummaryViewCell), NSBundle.MainBundle);
        }

        protected ChatSummaryViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            UnreadMessageCountBackground.Layer.MasksToBounds = true;
            UnreadMessageCountBackground.Layer.CornerRadius = 9;

            SenderPhotoImageView.MakeImageViewCircular();
        }

        public void BindViewModel(ChatSummaryViewModel viewModel)
        {
            _viewModelRef = WeakReferenceEx.Create(viewModel);

            LastMessageBodyPhotoIcon.Image = UIImage.FromBundle(StyleHelper.Style.LastMessageBodyPhotoIcon);
            LastMessageBodyPhotoLabel.Text = viewModel.LocalizedStrings.Photo;

            _bindings.Apply(x => x.Detach());
            _bindings.Clear();

            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.ChatName, () => ChatNameLabel.Text));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageViewModel.Body).WhenSourceChanges((obj) =>
            {
                MessageBodyLabel.Text = _viewModelRef.Target.LastMessageViewModel.Body;
                BodyContainer.Hidden = !_viewModelRef.Target.LastMessageViewModel.HasBody && !_viewModelRef.Target.LastMessageViewModel.HasPhoto;
            }));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.ChatPhotoUrl).WhenSourceChanges(() =>
            {
                SenderPhotoImageView.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.ChatPhotoUrl,
                    _viewModelRef.Target.ChatName,
                    StyleHelper.Style.AvatarStyles);
            }));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsMuted).WhenSourceChanges(() =>
            {
                if (UnreadMessageCountLabel != null)
                {
                    UnreadMessageCountBackground.BackgroundColor = _viewModelRef.Target.IsMuted
                        ? UIColor.FromRGB(180, 180, 180)
                        : StyleHelper.Style.AccentColor;
                }
            }));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.UnreadMessageCount).WhenSourceChanges(() =>
            {
                if (UnreadMessageCountLabel != null)
                {
                    UnreadMessageCountLabel.Text = _viewModelRef.Target.UnreadMessageCount.ToString();
                    UnreadMessageCountBackground.Hidden = _viewModelRef.Target.UnreadMessageCount == 0;
                }
            }));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageViewModel.DateTime, () => DateTimeLabel.Text));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.UnreadMessageCount, () => UnreadMessageCountLabel.Text));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageViewModel.HasPhoto).WhenSourceChanges(() =>
            {
                if (!_viewModelRef.Target.LastMessageViewModel.HasBody && _viewModelRef.Target.LastMessageViewModel.HasPhoto)
                {
                    LastMessageBodyPhotoView.Hidden = false;
                }
                else
                {
                    LastMessageBodyPhotoView.Hidden = true;
                }
                BodyContainer.Hidden = !_viewModelRef.Target.LastMessageViewModel.HasBody && !_viewModelRef.Target.LastMessageViewModel.HasPhoto;
            }));
        }
    }
}
