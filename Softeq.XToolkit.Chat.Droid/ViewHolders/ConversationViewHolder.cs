// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.ComponentModel;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Cross;
using FFImageLoading.Transformations;
using FFImageLoading.Work;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Controls;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.WeakSubscription;
using Softeq.XToolkit.WhiteLabel.Droid.Extensions;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.WhiteLabel.ViewModels;
using PopupMenu = Android.Support.V7.Widget.PopupMenu;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class ConversationViewHolder : BindableViewHolder<ChatMessageViewModel>
    {
        private readonly IDisposable _messageLongClickSubscription;
        private readonly bool _isIncomingMessageViewType;
        private readonly ContextMenuComponent _contextMenuComponent;

        private WeakReferenceEx<ChatMessageViewModel> _viewModelRef;

        public ConversationViewHolder(
            View itemView,
            bool isIncomingMessageViewType,
            ContextMenuComponent contextMenuComponent)
            : base(itemView)
        {
            _isIncomingMessageViewType = isIncomingMessageViewType;
            _contextMenuComponent = contextMenuComponent;

            MessageContainer = itemView.FindViewById<LinearLayout>(Resource.Id.ll_message_container);
            MessageBodyTextView = itemView.FindViewById<TextView>(Resource.Id.tv_message_body);
            MessageDateTimeTextView = itemView.FindViewById<TextView>(Resource.Id.tv_message_date_time);
            AttachmentImageView = itemView.FindViewById<MvxCachedImageView>(Resource.Id.iv_message_attachment);
            AttachmentImageView.Click += OnMessageImageClicked;

            var resourceId = _isIncomingMessageViewType
                ? StyleHelper.Style.IncomingMessageBg
                : StyleHelper.Style.OutcomingMessageBg;

            var imageBg = itemView.FindViewById<ImageView>(Resource.Id.item_chat_conversation_bg);
            imageBg.SetBackgroundResource(resourceId);

            // setup ViewHolder for in/outcoming messages

            if (_isIncomingMessageViewType)
            {
                SenderPhotoImageView = itemView.FindViewById<MvxCachedImageView>(Resource.Id.iv_sender_photo);
            }
            else
            {
                _messageLongClickSubscription = new WeakEventSubscription<LinearLayout, View.LongClickEventArgs>(
                    MessageContainer, nameof(MessageContainer.LongClick), MessageContainerLongClickHandler);

                MessageStatusView = itemView.FindViewById<ImageView>(Resource.Id.iv_message_status);
            }
        }

        private LinearLayout MessageContainer { get; }
        private TextView MessageBodyTextView { get; }
        private TextView MessageDateTimeTextView { get; }
        private MvxCachedImageView AttachmentImageView { get; }
        private MvxCachedImageView SenderPhotoImageView { get; }
        private ImageView MessageStatusView { get; }

        public override void BindViewModel(ChatMessageViewModel viewModel)
        {
            _viewModelRef = WeakReferenceEx.Create(viewModel);

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.Body).WhenSourceChanges(() =>
            {
                // TODO: check
                Execute.OnUIThread(() =>
                {
                    MessageBodyTextView.Text = _viewModelRef.Target.Body;
                });
            }));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.TextDateTime).WhenSourceChanges(() =>
            {
                // TODO: check
                Execute.OnUIThread(() =>
                {
                    MessageDateTimeTextView.Text = _viewModelRef.Target.TextDateTime;
                });
            }));

            if (_viewModelRef.Target.HasAttachment)
            {
                AttachmentImageView.Visibility = ViewStates.Visible;
                ImageService.Instance.LoadUrl(_viewModelRef.Target.AttachmentImageUrl)
                            .DownSampleInDip(90, 90)
                            .IntoAsync(AttachmentImageView);
            }
            else
            {
                AttachmentImageView.Visibility = ViewStates.Gone;
                AttachmentImageView.SetImageDrawable(null);
            }

            if (_isIncomingMessageViewType && SenderPhotoImageView != null)
            {
                SenderPhotoImageView.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.SenderPhotoUrl,
                    _viewModelRef.Target.SenderName,
                    new WhiteLabel.Droid.Controls.AvatarPlaceholderDrawable.AvatarStyles {
                        BackgroundHexColors = StyleHelper.Style.ChatAvatarStyles.BackgroundHexColors,
                        Size = new System.Drawing.Size(35, 35)
                    },
                    (TaskParameter x) => x.Transform(new CircleTransformation()));
            }

            if (!_isIncomingMessageViewType && MessageStatusView != null)
            {
                Bindings.Add(this.SetBinding(() => _viewModelRef.Target.Status).WhenSourceChanges(() =>
                {
                    // TODO: check
                    Execute.OnUIThread(() =>
                    {
                        if (_viewModelRef.Target == null)
                        {
                            return;
                        }

                        ChangeMessageViewStatus(_viewModelRef.Target.Status);
                    });
                }));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _messageLongClickSubscription?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void MessageContainerLongClickHandler(object sender, View.LongClickEventArgs eventArgs)
        {
            CreatePopupMenuFor((View)sender).Show();
        }

        private PopupMenu CreatePopupMenuFor(View itemView)
        {
            var popup = _contextMenuComponent.BuildMenu(itemView.Context, itemView);
            popup.MenuItemClick += PopupMenuItemClickHandler;
            return popup;
        }

        private void PopupMenuItemClickHandler(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            _contextMenuComponent.ExecuteCommand(e.Item.ItemId, _viewModelRef.Target);
        }

        private void ChangeMessageViewStatus(ChatMessageStatus status)
        {
            MessageStatusView.Visibility = ViewStates.Visible;

            int statusImageResourceId;
            switch (status)
            {
                case ChatMessageStatus.Sending:
                    statusImageResourceId = StyleHelper.Style.MessageStatusSentIcon;
                    break;
                case ChatMessageStatus.Delivered:
                    statusImageResourceId = StyleHelper.Style.MessageStatusDeliveredIcon;
                    break;
                case ChatMessageStatus.Read:
                    statusImageResourceId = StyleHelper.Style.MessageStatusReadIcon;
                    break;
                case ChatMessageStatus.Other:
                    MessageStatusView.Visibility = ViewStates.Gone;
                    return;
                default: throw new InvalidEnumArgumentException();
            }

            MessageStatusView.SetImageResource(statusImageResourceId);
        }

        private void OnMessageImageClicked(object sender, EventArgs e)
        {
            var options = new FullScreenImageOptions
            {
                ImageUrl = _viewModelRef.Target?.Model?.ImageUrl,
                DroidCloseButtonImageResId = Resource.Drawable.core_ic_close
            };
            _viewModelRef.Target?.ShowImage(options);
        }
    }
}
