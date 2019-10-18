// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using FFImageLoading.Cross;
using FFImageLoading.Transformations;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Commands;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.Common.Weak;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Extensions;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class ChatViewHolder : BindableViewHolder<ChatSummaryViewModel>
    {
        //TODO: move this to resources
        private const string ChatStatusDefaultColor = "#dedede";

        private readonly WeakAction<ChatSummaryViewModel> _selectChatAction;
        private readonly MvxCachedImageView _chatPhotoImageView;
        private readonly TextView _chatNameTextView;
        private readonly TextView _userNameTextView;
        private readonly TextView _messageBodyTextView;
        private readonly LinearLayout _messageBodyPhotoView;
        private readonly ImageView _messageBodyPhotoImageView;
        private readonly TextView _messageBodyPhotoLabel;
        private readonly TextView _dateTimeTextView;
        private readonly TextView _unreadMessageCountTextView;
        private readonly View _messageStatusIndicatorView;

        private WeakReferenceEx<ChatSummaryViewModel> _viewModelRef;

        public ChatViewHolder(View itemView, Action<ChatSummaryViewModel> selectChatAction) : base(itemView)
        {
            _selectChatAction = new WeakAction<ChatSummaryViewModel>(selectChatAction);

            _chatPhotoImageView = itemView.FindViewById<MvxCachedImageView>(Resource.Id.chat_photo_image_view);
            _chatNameTextView = itemView.FindViewById<TextView>(Resource.Id.chat_name_text_view);
            _userNameTextView = itemView.FindViewById<TextView>(Resource.Id.username_text_view);
            _messageBodyTextView = itemView.FindViewById<TextView>(Resource.Id.message_body_text_view);
            _dateTimeTextView = itemView.FindViewById<TextView>(Resource.Id.date_time_text_view);
            _unreadMessageCountTextView = itemView.FindViewById<TextView>(Resource.Id.unreaded_messages_count_text_view);
            _messageStatusIndicatorView = itemView.FindViewById<View>(Resource.Id.message_status_indicator);
            _messageBodyPhotoView = itemView.FindViewById<LinearLayout>(Resource.Id.chat_message_body_photo_view);
            _messageBodyPhotoImageView = itemView.FindViewById<ImageView>(Resource.Id.chat_message_body_photo_icon);
            _messageBodyPhotoLabel = itemView.FindViewById<TextView>(Resource.Id.chat_message_body_photo_label);

            itemView.SetCommand(nameof(itemView.Click), new RelayCommand(ChatClickHandler));
        }

        public override void BindViewModel(ChatSummaryViewModel viewModel)
        {
            _viewModelRef = WeakReferenceEx.Create(viewModel);

            _messageBodyPhotoLabel.Text = viewModel.LocalizedStrings.Photo;
            _messageBodyPhotoImageView.SetImageResource(StyleHelper.Style.LastMessageBodyPhotoIcon);

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.ChatName, () => _chatNameTextView.Text));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageViewModel.Username, () => _userNameTextView.Text));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageViewModel.Body).WhenSourceChanges(() =>
            {
                _messageBodyTextView.Text = _viewModelRef.Target.LastMessageViewModel.Body;
                _messageBodyTextView.Visibility = BoolToViewStateConverter.ConvertGone(_viewModelRef.Target.LastMessageViewModel.HasBody);
            }));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageViewModel.DateTime, () => _dateTimeTextView.Text));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.ChatPhotoUrl).WhenSourceChanges(() =>
            {
                if (_chatPhotoImageView == null)
                {
                    return;
                }

                _chatPhotoImageView.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.ChatPhotoUrl,
                    _viewModelRef.Target.ChatName,
                    new AvatarPlaceholderDrawable.AvatarStyles
                    {
                        BackgroundHexColors = StyleHelper.Style.ChatAvatarStyles.BackgroundHexColors,
                        Size = new System.Drawing.Size(44, 44)
                    },
                    x => x.Transform(new CircleTransformation()));
            }));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.UnreadMessageCount).WhenSourceChanges(() =>
            {
                if (_unreadMessageCountTextView != null)
                {
                    _unreadMessageCountTextView.Text = _viewModelRef.Target.UnreadMessageCount.ToString();
                    _unreadMessageCountTextView.Visibility = BoolToViewStateConverter.ConvertGone(_viewModelRef.Target.UnreadMessageCount > 0);
                }
            }));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsMuted).WhenSourceChanges(() =>
            {
                if (_unreadMessageCountTextView != null)
                {
                    var colorResId = _viewModelRef.Target.IsMuted
                        ? StyleHelper.Style.UnreadMutedMessagesCountColor
                        : StyleHelper.Style.UnreadMessagesCountColor;

                    var color = ContextCompat.GetColor(_unreadMessageCountTextView.Context, colorResId);

                    _unreadMessageCountTextView.Background = CreateBackgroundWithCornerRadius(color, 56f);
                }
            }));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageViewModel.Status).WhenSourceChanges(() =>
            {
                Color color;

                switch (_viewModelRef.Target.LastMessageViewModel.Status)
                {
                    case ChatMessageStatus.Read:
                        color = Color.GreenYellow;
                        break;
                    case ChatMessageStatus.Other:
                        color = Color.Transparent;
                        break;
                    default:
                        color = Color.ParseColor(ChatStatusDefaultColor);
                        break;
                }

                _messageStatusIndicatorView?.SetBackgroundColor(color);
            }));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageViewModel.HasPhoto).WhenSourceChanges(() =>
            {
                if (!_viewModelRef.Target.LastMessageViewModel.HasBody && _viewModelRef.Target.LastMessageViewModel.HasPhoto)
                {
                    _messageBodyTextView.Visibility = ViewStates.Gone;
                    _messageBodyPhotoView.Visibility = ViewStates.Visible;
                }
                else
                {
                    _messageBodyTextView.Visibility = BoolToViewStateConverter.ConvertGone(_viewModelRef.Target.LastMessageViewModel.HasBody);
                    _messageBodyPhotoView.Visibility = ViewStates.Gone;
                }
            }));
        }

        // TODO: move to XToolkit.Common
        private static Drawable CreateBackgroundWithCornerRadius(int color, float radius)
        {
            var drawable = new GradientDrawable();

            drawable.SetShape(ShapeType.Rectangle);
            drawable.SetColor(color);
            drawable.SetCornerRadius(radius);

            return drawable;
        }

        private void ChatClickHandler()
        {
            _selectChatAction.Execute(_viewModelRef.Target);
        }
    }
}
