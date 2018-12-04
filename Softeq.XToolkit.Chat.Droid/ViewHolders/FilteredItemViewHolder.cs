// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading.Transformations;
using FFImageLoading.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.Droid.Extensions;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class FilteredItemViewHolder : RecyclerView.ViewHolder
    {
        private readonly ImageViewAsync _avatar;
        private readonly ImageView _imageView;
        private readonly TextView _textView;
        private readonly View _view;

        private Binding _selectedBinding;
        private ChatUserViewModel _viewModel;

        public FilteredItemViewHolder(View itemView) : base(itemView)
        {
            _imageView = itemView.FindViewById<ImageView>(Resource.Id.view_holder_member_filter_item_indicator);
            _avatar = itemView.FindViewById<ImageViewAsync>(Resource.Id.view_holder_member_filter_item_avatar);
            _textView = itemView.FindViewById<TextView>(Resource.Id.view_holder_member_filter_item_title);
            _view = itemView;
            _view.Click += ViewOnClick;
        }

        private void ViewOnClick(object sender, EventArgs e)
        {
            _viewModel.IsSelected = !_viewModel.IsSelected;
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

            _selectedBinding?.Detach();
            _selectedBinding = this.SetBinding(() => _viewModel.IsSelected).WhenSourceChanges(() =>
            {
                var resId = _viewModel.IsSelected ? StyleHelper.Style.CheckedIcon : StyleHelper.Style.UnCheckedIcon;
                _imageView.SetImageResource(resId);
            });
        }
    }
}
