﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Cross;
using FFImageLoading.Transformations;
using FFImageLoading.Work;
using Plugin.CurrentActivity;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Controls;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.Common.Droid.Extensions;
using Softeq.XToolkit.Common.Weak;
using Softeq.XToolkit.WhiteLabel.Droid.Extensions;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.WhiteLabel.ViewModels;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class ConversationViewHolder : BindableViewHolder<ChatMessageViewModel>
    {
        private readonly IDisposable _messageLongClickSubscription;
        private readonly bool _isIncomingMessageViewType;
        private readonly IItemActionHandler<ChatMessageViewModel> _actionHandler;
        private WeakReferenceEx<ChatMessageViewModel> _viewModelRef;

        public ConversationViewHolder(
            View itemView,
            bool isIncomingMessageViewType,
            IItemActionHandler<ChatMessageViewModel> actionHandler)
            : base(itemView)
        {
            _isIncomingMessageViewType = isIncomingMessageViewType;
            _actionHandler = actionHandler;

            MessageContainer = itemView.FindViewById<LinearLayout>(Resource.Id.ll_message_container);
            MessageBodyTextView = itemView.FindViewById<TextView>(Resource.Id.tv_message_body);
            MessageDateTimeTextView = itemView.FindViewById<TextView>(Resource.Id.tv_message_date_time);
            AttachmentImageView = itemView.FindViewById<MvxCachedImageView>(Resource.Id.iv_message_attachment);
            AttachmentImageView.Click += OnMessageImageClicked;

            AttachmentImageView.SetAdjustViewBounds(true);
            AttachmentImageView.SetScaleType(ImageView.ScaleType.CenterCrop);

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

        private TaskParameter _downloadAttachTask;
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
                    var hideMessageBody = string.IsNullOrEmpty(_viewModelRef.Target.Body) && _viewModelRef.Target.HasAttachment;
                    MessageBodyTextView.Visibility = BoolToViewStateConverter.ConvertGone(hideMessageBody == false);
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
                var model = _viewModelRef.Target.Model;
                var expr = default(TaskParameter);

                AttachmentImageView.SetImageResource(StyleHelper.Style.AttachmentImagePlaceholder);
                UpdateAttachmentImageViewSizeAndVisibility();

                if (!string.IsNullOrEmpty(model.ImageCacheKey))
                {
                    expr = ImageService.Instance.LoadFile(model.ImageCacheKey);
                }
                else if (!string.IsNullOrEmpty(model.ImageRemoteUrl))
                {
                    expr = ImageService.Instance.LoadUrl(model.ImageRemoteUrl);
                }

                if (expr == null)
                {
                    return;
                }

                _downloadAttachTask = expr.DownSampleInDip(90, 90)
                    .Finish(x =>
                    {
                        Execute.BeginOnUIThread(UpdateAttachmentImageViewSizeAndVisibility);
                    });
                _downloadAttachTask.IntoAsync(AttachmentImageView);
            }
            else
            {
                _downloadAttachTask?.Dispose();
                AttachmentImageView.SetImageDrawable(null);
                AttachmentImageView.Visibility = ViewStates.Gone;
            }

            if (_isIncomingMessageViewType && SenderPhotoImageView != null)
            {
                SenderPhotoImageView.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.SenderPhotoUrl,
                    _viewModelRef.Target.SenderName,
                    new WhiteLabel.Droid.Controls.AvatarPlaceholderDrawable.AvatarStyles
                    {
                        BackgroundHexColors = StyleHelper.Style.ChatAvatarStyles.BackgroundHexColors,
                        Size = new System.Drawing.Size(35, 35)
                    },
                    x => x.Transform(new CircleTransformation()));
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
                _downloadAttachTask?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void MessageContainerLongClickHandler(object sender, View.LongClickEventArgs eventArgs)
        {
            _actionHandler.Handle((View) sender, _viewModelRef.Target);
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
            if (sender is MvxCachedImageView mvxCachedImage && mvxCachedImage.Drawable is VectorDrawable)
            {
                return; // ignore placeholder
            }

            var url = _viewModelRef.Target?.Model?.ImageRemoteUrl;
            if (url == null)
            {
                return;
            }

            var options = new FullScreenImageOptions
            {
                ImageUrl = url,
                DroidCloseButtonImageResId = Resource.Drawable.core_ic_close
            };
            _viewModelRef.Target?.ShowImage(options);
        }

        private void UpdateAttachmentImageViewSizeAndVisibility()
        {
            var lp = AttachmentImageView.LayoutParameters;
            (lp.Width, lp.Height) = CalculateAttachmentImageViewSize();
            AttachmentImageView.LayoutParameters = lp;

            AttachmentImageView.Visibility = ViewStates.Visible;
        }

        protected virtual (int Width, int Height) CalculateAttachmentImageViewSize()
        {
            var defSize = (ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            var context = CrossCurrentActivity.Current.AppContext;

            if (AttachmentImageView?.Drawable == null)
            {
                return defSize;
            }

            var originalImageWidth = context.ToPixels(AttachmentImageView.Drawable.IntrinsicWidth);
            var originalImageHeight = context.ToPixels(AttachmentImageView.Drawable.IntrinsicHeight);

            if (originalImageWidth <= 0 || originalImageHeight <= 0)
            {
                return defSize;
            }

            var maxImageWidth = GetAttachmentImageViewMaxWidth(context);

            var ratio = originalImageWidth / originalImageHeight;

            var isLandscape = originalImageWidth > originalImageHeight;

            if (isLandscape)
            {
                var calculatedHeight = maxImageWidth / ratio;
                return (maxImageWidth, (int) calculatedHeight);
            }
            else
            {
                var calculatedWidth = maxImageWidth * ratio;
                return ((int) calculatedWidth, maxImageWidth);
            }
        }

        protected virtual int GetAttachmentImageViewMaxWidth(Context context)
        {
            var displayWidth = context.ApplicationContext.Resources.DisplayMetrics.WidthPixels;
            var paddingsAndMarginsOfContainer = 64;

            if (_isIncomingMessageViewType)
            {
                paddingsAndMarginsOfContainer += 35 + 16 + 24 + 12;
            }
            else
            {
                paddingsAndMarginsOfContainer += 20 + 20 + 8;
            }

            return displayWidth - (int) context.ToPixels(paddingsAndMarginsOfContainer);
        }
    }
}
