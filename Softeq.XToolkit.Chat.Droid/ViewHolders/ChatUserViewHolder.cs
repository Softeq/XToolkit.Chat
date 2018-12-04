// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.Views;
using Android.Widget;
using FFImageLoading.Cross;
using FFImageLoading.Transformations;
using FFImageLoading.Work;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.WhiteLabel.Droid.Extensions;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class ChatUserViewHolder : BindableViewHolder<ChatUserViewModel>
    {
        private WeakReferenceEx<ChatUserViewModel> _viewModelRef;

        public ChatUserViewHolder(View itemView) : base(itemView)
        {
            ContactPhotoImageView = itemView.FindViewById<MvxCachedImageView>(Resource.Id.iv_contact_photo);
            ContactUserNameTextView = itemView.FindViewById<TextView>(Resource.Id.tv_contact_username);
            ContactSwitch = itemView.FindViewById<Switch>(Resource.Id.switch_contact_isselected);
        }

        private MvxCachedImageView ContactPhotoImageView { get; }
        private TextView ContactUserNameTextView { get; }
        public Switch ContactSwitch { get; }

        public override void BindViewModel(ChatUserViewModel viewModel)
        {
            _viewModelRef = WeakReferenceEx.Create(viewModel);

            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.Username, () => ContactUserNameTextView.Text));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsSelected, () => ContactSwitch.Checked, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => _viewModelRef.Target.PhotoUrl).WhenSourceChanges(() =>
            {
                ContactPhotoImageView.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.PhotoUrl,
                    _viewModelRef.Target.Username,
                    StyleHelper.Style.ChatAvatarStyles,
                    (TaskParameter x) => x.Transform(new CircleTransformation()));
            }));
        }
    }
}
