﻿// Developed by Softeq Development Corporation
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
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.WhiteLabel.Droid.Services;
using System;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity]
    public class ChatDetailsActivity : ActivityBase<ChatDetailsViewModel>
    {
        private NavigationBarView _navigationBarView;
        private MvxCachedImageView _chatPhotoImageView;
        private MvxCachedImageView _chatEditedPhotoImageView;
        private EditText _chatNameEditText;
        private TextView _chatMembersCountTextView;
        private Button _addMemberButton;
        private RecyclerView _membersRecyclerView;
        private Button _changeChatPhotoButton;
        private ImagePicker _imagePicker;
        private string _previewImageKey;
        private TextView _muteNotificationsLabel;
        private SwitchCompat _muteNotificationsSwitch;
        private BusyOverlayView _busyOverlayView;

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
            _navigationBarView.RightTextButton.Visibility = ViewStates.Gone;

            _chatPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);

            _chatEditedPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo_edited);

            _chatNameEditText = FindViewById<EditText>(Resource.Id.activity_chat_details_chat_name);

            _chatMembersCountTextView = FindViewById<TextView>(Resource.Id.tv_members_count);

            _addMemberButton = FindViewById<Button>(Resource.Id.b_chat_add_member);
            _addMemberButton.SetCommand(ViewModel.AddMembersCommand);
            _addMemberButton.Text = ViewModel.LocalizedStrings.AddMembers;

            _membersRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);

            _changeChatPhotoButton = FindViewById<Button>(Resource.Id.b_chat_change_photo);
            _changeChatPhotoButton.SetCommand(new RelayCommand(OpenImagePicker));
            _changeChatPhotoButton.Text = ViewModel.LocalizedStrings.ChangePhoto;

            _muteNotificationsLabel = FindViewById<TextView>(Resource.Id.activity_chat_details_mute_label);
            _muteNotificationsLabel.Text = ViewModel.LocalizedStrings.Notifications;
            _muteNotificationsSwitch = FindViewById<SwitchCompat>(Resource.Id.activity_chat_details_mute_switch);
            _muteNotificationsSwitch.SetCommand(ViewModel.ChangeMuteNotificationsCommand);

            InitializeMembersRecyclerView();

            // TODO YP: remove ServiceLocator
            _imagePicker = new ImagePicker(
                Dependencies.IocContainer.Resolve<IPermissionsManager>(),
                Dependencies.IocContainer.Resolve<IImagePickerService>())
            {
                MaxImageWidth = 300
            };

            _busyOverlayView = FindViewById<BusyOverlayView>(Resource.Id.activity_chat_details_busy_view);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatNameEditText.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.MembersCountText, () => _chatMembersCountTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.AvatarUrl).WhenSourceChanges(() =>
            {
                Execute.BeginOnUIThread(() =>
                {
                    _chatPhotoImageView.LoadImageWithTextPlaceholder(
                        ViewModel.AvatarUrl,
                        ViewModel.ChatName,
                        new AvatarPlaceholderDrawable.AvatarStyles
                        {
                            BackgroundHexColors = StyleHelper.Style.ChatAvatarStyles.BackgroundHexColors,
                            Size = new System.Drawing.Size(64, 64)
                        },
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
                        ViewModel.IsInEditMode = true;

                        ImageService.Instance
                            .LoadFile(_imagePicker.ViewModel.ImageCacheKey)
                            .DownSampleInDip(95, 95)
                            .Transform(new CircleTransformation())
                            .IntoAsync(_chatEditedPhotoImageView);
                    });
                }));
            Bindings.Add(this.SetBinding(() => ViewModel.IsMuted, () => _muteNotificationsSwitch.Checked)
                .ConvertSourceToTarget(x => !x));
            Bindings.Add(this.SetBinding(() => ViewModel.IsBusy, () => _muteNotificationsSwitch.Enabled)
                .ConvertSourceToTarget(x => !x));
            Bindings.Add(this.SetBinding(() => ViewModel.IsLoading).WhenSourceChanges(() =>
            {
                _busyOverlayView.Visibility = BoolToViewStateConverter.ConvertGone(ViewModel.IsLoading);
                _chatMembersCountTextView.Visibility = BoolToViewStateConverter.ConvertGone(!ViewModel.IsLoading);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.CanEdit, BindingMode.OneTime).WhenSourceChanges(() =>
            {
                _changeChatPhotoButton.Visibility = BoolToViewStateConverter.ConvertGone(ViewModel.CanEdit);
                _chatNameEditText.Enabled = ViewModel.CanEdit;

                if (!ViewModel.CanEdit)
                {
                    _chatNameEditText.SetBackgroundColor(Color.Transparent);
                }
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.IsInEditMode).WhenSourceChanges(() =>
            {
                if (ViewModel.IsInEditMode)
                {
                    _navigationBarView.RightTextButton.Visibility = ViewStates.Visible;
                    _chatEditedPhotoImageView.Visibility = ViewStates.Visible;
                }
                else
                {
                    _previewImageKey = null;
                    _navigationBarView.RightTextButton.Visibility = ViewStates.Gone;
                    _chatEditedPhotoImageView.Visibility = ViewStates.Gone;

                    _chatNameEditText.ClearFocus();
                    KeyboardService.HideSoftKeyboard(_chatNameEditText);
                }
            }));

            _chatNameEditText.FocusChange += OnEditTextFocusChanged;
        }

        protected override void DoDetachBindings()
        {
            base.DoDetachBindings();

            _chatNameEditText.FocusChange -= OnEditTextFocusChanged;
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
            var member = ViewModel.Members[position];
            if (!member.IsRemovable)
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
                    ViewModel.RemoveMemberCommand.Execute(ViewModel.Members[pos]);
                }));
        }

        private void OnEditTextFocusChanged(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                ViewModel.IsInEditMode = true;
            }
        }

        private void OpenImagePicker()
        {
            _imagePicker.OpenGallery();
        }

        private void OnSaveClick()
        {
            ViewModel.SaveCommand.Execute(_imagePicker.GetStreamFunc());
        }
    }
}