// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading.Transformations;
using FFImageLoading.Views;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.Droid.Shared.Extensions;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class SelectedContactViewHolder : RecyclerView.ViewHolder
    {
        private readonly ImageViewAsync _avatar;
        private readonly ImageButton _removeButton;
        private readonly TextView _textView;

        private ChatUserViewModel _viewModel;

        public SelectedContactViewHolder(View itemView) : base(itemView)
        {
            _avatar = itemView.FindViewById<ImageViewAsync>(Resource.Id.view_holder_member_avatar);
            _removeButton = itemView.FindViewById<ImageButton>(Resource.Id.view_holder_member_remove);
            _removeButton.SetImageResource(StyleHelper.Style.RemoveImageButtonIcon);
            _removeButton.SetBackgroundResource(Android.Resource.Color.Transparent);
            _removeButton.Click += RemoveButtonOnClick;
            _textView = itemView.FindViewById<TextView>(Resource.Id.view_holder_member_name);
        }

        private void RemoveButtonOnClick(object sender, EventArgs e)
        {
            _viewModel.IsSelected = false;
        }

        public void Bind(ChatUserViewModel viewModel)
        {
            _viewModel = viewModel;
            _avatar.LoadImageWithTextPlaceholder(
                viewModel.PhotoUrl,
                viewModel.Username,
                StyleHelper.Style.ChatAvatarStyles,
                x => x.Transform(new CircleTransformation()));
            _textView.Text = viewModel.Username;
        }
    }
}
