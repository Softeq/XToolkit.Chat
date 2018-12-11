// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Content;
using FFImageLoading.Cross;
using FFImageLoading.Transformations;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Extensions;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class ChatViewHolder : BindableViewHolder<ChatSummaryViewModel>
    {
        //TODO: move this to resources
        private const string ChatStatusDefaultColor = "#dedede";

        private readonly WeakAction<ChatSummaryViewModel> _selectChatAction;

        private WeakReferenceEx<ChatSummaryViewModel> _viewModelRef;

        public ChatViewHolder(View itemView, Action<ChatSummaryViewModel> selectChatAction) : base(itemView)
        {
            _selectChatAction = new WeakAction<ChatSummaryViewModel>(selectChatAction);

            ChatPhotoImageView = itemView.FindViewById<MvxCachedImageView>(Resource.Id.chat_photo_image_view);
            ChatNameTextView = itemView.FindViewById<TextView>(Resource.Id.chat_name_text_view);
            UserNameTextView = itemView.FindViewById<TextView>(Resource.Id.username_text_view);
            MessageBodyTextView = itemView.FindViewById<TextView>(Resource.Id.message_body_text_view);
            DateTimeTextView = itemView.FindViewById<TextView>(Resource.Id.date_time_text_view);
            UnreadMessageCountTextView = itemView.FindViewById<TextView>(Resource.Id.unreaded_messages_count_text_view);
            MessageStatusIndicatorView = itemView.FindViewById<View>(Resource.Id.message_status_indicator);

            itemView.SetCommand(nameof(itemView.Click), new RelayCommand(ChatClickHandler));
        }

        private MvxCachedImageView ChatPhotoImageView { get; }
        private TextView ChatNameTextView { get; }
        private TextView UserNameTextView { get; }
        private TextView MessageBodyTextView { get; }
        private TextView DateTimeTextView { get; }
        private TextView UnreadMessageCountTextView { get; }
        private View MessageStatusIndicatorView { get; }

        public override void BindViewModel(ChatSummaryViewModel viewModel)
        {
            _viewModelRef = WeakReferenceEx.Create(viewModel);

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.ChatName, () => ChatNameTextView.Text));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageUsername, () => UserNameTextView.Text));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageBody, () => MessageBodyTextView.Text));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageDateTime, () => DateTimeTextView.Text));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.ChatPhotoUrl).WhenSourceChanges(() =>
            {
                if (ChatPhotoImageView == null)
                {
                    return;
                }

                ChatPhotoImageView.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.ChatPhotoUrl,
                    _viewModelRef.Target.ChatName,
                    new AvatarPlaceholderDrawable.AvatarStyles
                    {
                        BackgroundHexColors = StyleHelper.Style.ChatAvatarStyles.BackgroundHexColors,
                        Size = new System.Drawing.Size(45, 45)
                    },
                    x => x.Transform(new CircleTransformation()));
            }));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.UnreadMessageCount).WhenSourceChanges(() =>
            {
                if (UnreadMessageCountTextView != null)
                {
                    UnreadMessageCountTextView.Text = _viewModelRef.Target.UnreadMessageCount.ToString();
                    UnreadMessageCountTextView.Visibility = BoolToViewStateConverter.ConvertGone(_viewModelRef.Target.UnreadMessageCount > 0);
                }
            }));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsMuted).WhenSourceChanges(() =>
            {
                if (UnreadMessageCountTextView != null)
                {
                    var colorResId = _viewModelRef.Target.IsMuted
                        ? StyleHelper.Style.UnreadMutedMessagesCountColor
                        : StyleHelper.Style.UnreadMessagesCountColor;

                    var color = ContextCompat.GetColor(UnreadMessageCountTextView.Context, colorResId);

                    UnreadMessageCountTextView.Background = CreateBackgroundWithCornerRadius(color, 56f);
                }
            }));

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.LastMessageStatus).WhenSourceChanges(() =>
            {
                Color color;

                switch (_viewModelRef.Target.LastMessageStatus)
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

                MessageStatusIndicatorView?.SetBackgroundColor(color);
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
