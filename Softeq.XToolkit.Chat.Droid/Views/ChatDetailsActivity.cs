// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Cross;
using FFImageLoading.Transformations;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Adapters;
using Softeq.XToolkit.Chat.Droid.ItemTouchCallbacks;
using Softeq.XToolkit.Chat.Droid.LayoutManagers;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Droid.Extensions;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Extensions;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity]
    public class ChatDetailsActivity : ActivityBase<ChatDetailsViewModel>
    {
        private NavigationBarView _navigationBarView;
        private MvxCachedImageView _chatPhotoImageView;
        private MvxCachedImageView _chatEditedPhotoImageView;
        private TextView _chatNameTextView;
        private TextView _chatMembersCountTextView;
        private Button _addMemberButton;
        private RecyclerView _membersRecyclerView;
        private Button _changeChatPhotoButton;
        private ImagePicker _imagePicker;
        private string _previewImageKey;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            OverridePendingTransition(0, 0);

            SetTheme(StyleHelper.Style.CommonActivityStyle);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat_details);

            _navigationBarView = FindViewById<NavigationBarView>(Resource.Id.activity_chat_details_navigation_bar);
            _navigationBarView.SetLeftButton(StyleHelper.Style.NavigationBarBackButtonIcon, ViewModel.BackCommand);
            _navigationBarView.SetTitle(ViewModel.LocalizedStrings.DetailsTitle);
            _navigationBarView.SetRightButton(ViewModel.LocalizedStrings.Save, new RelayCommand(OnSaveClick));

            _chatPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);

            _chatEditedPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo_edited);
            _chatEditedPhotoImageView.Visibility = ViewStates.Gone;

            _chatNameTextView = FindViewById<TextView>(Resource.Id.tv_chat_name);
            _chatMembersCountTextView = FindViewById<TextView>(Resource.Id.tv_members_count);
            _addMemberButton = FindViewById<Button>(Resource.Id.b_chat_add_member);
            _membersRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);

            _changeChatPhotoButton = FindViewById<Button>(Resource.Id.b_chat_change_photo);
            _changeChatPhotoButton.SetCommand(new RelayCommand(OpenImagePicker));

            InitializeMembersRecyclerView();

            _addMemberButton.SetCommand(ViewModel.AddMembersCommand);

            // TODO YP: remove ServiceLocator
            _imagePicker = new ImagePicker(
                Dependencies.IocContainer.Resolve<IPermissionsManager>(),
                Dependencies.IocContainer.Resolve<IImagePickerService>())
            {
                MaxImageWidth = 300
            };
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.Summary.Name, () => _chatNameTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.MembersCountText, () => _chatMembersCountTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.Summary.AvatarUrl).WhenSourceChanges(() =>
            {
                Execute.BeginOnUIThread(() =>
                {
                    _chatPhotoImageView.LoadImageWithTextPlaceholder(
                        ViewModel.Summary.AvatarUrl,
                        ViewModel.Summary.Name,
                        StyleHelper.Style.ChatAvatarStyles,
                        x => x.Transform(new CircleTransformation()));
                });
            }));
            Bindings.Add(this.SetBinding(() => _imagePicker.ViewModel.ImageCacheKey)
                .WhenSourceChanges(() =>
                {
                    var key = _imagePicker.ViewModel.ImageCacheKey;
                    if (key == _previewImageKey)
                    {
                        return;
                    }

                    _previewImageKey = key;

                    Execute.BeginOnUIThread(() =>
                    {
                        _chatEditedPhotoImageView.Visibility = ViewStates.Visible;
                        ImageService.Instance
                            .LoadFile(_imagePicker.ViewModel.ImageCacheKey)
                            .DownSampleInDip(95, 95)
                            .Transform(new CircleTransformation())
                            .IntoAsync(_chatEditedPhotoImageView);
                    });
                }));
        }

        protected override void OnDestroy()
        {
            _membersRecyclerView.GetAdapter().Dispose();

            base.OnDestroy();
        }

        private void InitializeMembersRecyclerView()
        {
            var swipeItemCallback = new SwipeCallback(this, _membersRecyclerView, ConfigureSwipeForViewHolder);
            var swipeItemTouchHelper = new ItemTouchHelper(swipeItemCallback);
            swipeItemTouchHelper.AttachToRecyclerView(_membersRecyclerView);

            _membersRecyclerView.HasFixedSize = true;
            _membersRecyclerView.SetLayoutManager(new GuardedLinearLayoutManager(this));
            _membersRecyclerView.AddItemDecoration(new DividerItemDecoration(this, DividerItemDecoration.Vertical));
            _membersRecyclerView.SetAdapter(new BaseChatObservableRecyclerViewAdapter<ChatUserViewModel>(
                ViewModel.Members,
                x =>
                {
                    var itemView = LayoutInflater.From(x.Item1.Context)
                                                 .Inflate(Resource.Layout.item_chat_contact, x.Item1, false);

                    var viewHolder = new ChatUserViewHolder(itemView);
                    viewHolder.ContactSwitch.Visibility = ViewStates.Gone;
                    return viewHolder;
                }));
        }

        private void ConfigureSwipeForViewHolder(RecyclerView.ViewHolder viewHolder, int position,
            ICollection<SwipeCallback.ISwipeActionView> actions)
        {
            if (ViewModel.IsMemberRemovable(position))
            {
                return;
            }

            var swipeLeaveActionViewOptions = new SimpleSwipeActionView.Options
            {
                Width = this.ToPixels(80),
                TextSize = this.ToPixels(14),
                BackgroundColor = Color.Red
            };

            actions.Add(new SimpleSwipeActionView(ViewModel.LocalizedStrings.Remove,
                swipeLeaveActionViewOptions,
                pos =>
                {
                    ViewModel.RemoveMemberAtCommand.Execute(pos);
                }));
        }

        private void OpenImagePicker()
        {
            _imagePicker.OpenGallery();
        }

        private async void OnSaveClick()
        {
            await ViewModel.SaveAsync(_imagePicker.GetStreamFunc()).ConfigureAwait(false);
            Execute.BeginOnUIThread(() =>
            {
                _previewImageKey = null;
                _navigationBarView.RightTextButton.Visibility = ViewStates.Gone;
            });
        }
    }
}