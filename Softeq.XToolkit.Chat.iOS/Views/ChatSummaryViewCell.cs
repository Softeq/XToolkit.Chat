// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Foundation;
using UIKit;
using Softeq.XToolkit.Common;
using System.Collections.Generic;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.iOS.Helpers;
using Softeq.XToolkit.Chat.ViewModels;

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

        [Export("awakeFromNib")]
        new public void AwakeFromNib()
        {
            UnreadMessageCountBackground.Layer.MasksToBounds = true;
            UnreadMessageCountBackground.Layer.CornerRadius = 9;

            SenderPhotoImageView.MakeImageViewCircular();
        }

        public void BindViewModel(ChatSummaryViewModel viewModel)
        {
            _viewModelRef = WeakReferenceEx.Create(viewModel);

            _bindings.Apply(x => x.Detach());
            _bindings.Clear();

            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.ChatName, () => ChatNameLabel.Text));
            //_bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageUsername, () => UsernameLabel.Text));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageBody, () => MessageBodyLabel.Text));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.ChatPhotoUrl).WhenSourceChanges(() =>
            {
                SenderPhotoImageView.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.ChatPhotoUrl,
                    _viewModelRef.Target.ChatName,
                    StyleHelper.Style.AvatarStyles);
            }));
            //_bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageStatus).WhenSourceChanges(() =>
            //{
            //    if (ReadUnreadIndicator != null && UnreadView != null)
            //    {
            //        ReadUnreadIndicator.Hidden = _viewModelRef.Target.LastMessageStatus == ChatMessageStatus.Other;
            //        UnreadView.Hidden = _viewModelRef.Target.LastMessageStatus == ChatMessageStatus.Read;
            //    }
            //}));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsMuted).WhenSourceChanges(() =>
            {
                if (UnreadMessageCountLabel != null)
                {
                    UnreadMessageCountBackground.BackgroundColor = _viewModelRef.Target.IsMuted
                        ? UIColor.FromRGB(180, 180, 180)
                        : UIColor.FromRGB(91, 198, 201);
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
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageDateTime, () => DateTimeLabel.Text));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.UnreadMessageCount, () => UnreadMessageCountLabel.Text));
        }
    }
}
