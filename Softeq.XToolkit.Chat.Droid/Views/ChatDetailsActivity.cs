﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Cross;
using FFImageLoading.Transformations;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Adapters;
using Softeq.XToolkit.Chat.Droid.Extensions;
using Softeq.XToolkit.Chat.Droid.LayoutManagers;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Threading;
using AndroidResource = Android.Resource;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity(Theme = "@style/ChatTheme")]
    public class ChatDetailsActivity : ActivityBase<ChatDetailsViewModel>
    {
        private MvxCachedImageView _chatPhotoImageView;
        private MvxCachedImageView _chatEditedPhotoImageView;
        private TextView _chatNameTextView;
        private TextView _chatMembersCountTextView;
        private Button _addMemberButton;
        private RecyclerView _membersRecyclerView;
        private Button _changeChatPhotoButton;
        private ImagePicker _imagePicker;
        private string _previewImageKey;
        private ToolbarMenuComponent _toolbarMenuComponent;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            return RebuildMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            return RebuildMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var handled = _toolbarMenuComponent.HandleClick(item);
            if (handled)
            {
                return true;
            }
            
            if (item.ItemId == AndroidResource.Id.Home)
            {
                OnBackPressed();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            OverridePendingTransition(0, 0);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat_details);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar_chat_details);

            _chatPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);

            _chatEditedPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo_edited);
            _chatEditedPhotoImageView.Visibility = ViewStates.Gone;

            _chatNameTextView = FindViewById<TextView>(Resource.Id.tv_chat_name);
            _chatMembersCountTextView = FindViewById<TextView>(Resource.Id.tv_members_count);
            _addMemberButton = FindViewById<Button>(Resource.Id.b_chat_add_member);
            _membersRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);

            _changeChatPhotoButton = FindViewById<Button>(Resource.Id.b_chat_change_photo);
            _changeChatPhotoButton.SetCommand(new RelayCommand(OpenImagePicker));

            InitializeToolbar(toolbar);
            InitializeMembersRecyclerView();

            _addMemberButton.SetCommand(ViewModel.AddMembersCommand);

            _imagePicker = new ImagePicker(ServiceLocator.Resolve<IPermissionsManager>(), ServiceLocator.Resolve<IImagePickerService>())
            {
                MaxImageWidth = 300
            };
            
            _toolbarMenuComponent = new ToolbarMenuComponent();
            
            ViewModel.MenuActions.Clear();
            ViewModel.MenuActions.Add(new CommandAction
            {
                Title = ViewModel.LocalizedStrings.Save,
                CommandActionStyle = CommandActionStyle.Destructive,
                Command = new RelayCommand(OnSaveClick)
            });
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatNameTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.MembersCountText, () => _chatMembersCountTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.ChatAvatarUrl).WhenSourceChanges(() =>
            {
                _chatPhotoImageView.LoadImageAsync("ic_attachment.png", ViewModel.ChatAvatarUrl, new CircleTransformation());
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
                        InvalidateOptionsMenu();
                    });
                }));
        }

        protected override void OnDestroy()
        {
            _membersRecyclerView.GetAdapter().Dispose();

            base.OnDestroy();
        }

        private void InitializeToolbar(Toolbar toolbar)
        {
            SetSupportActionBar(toolbar);

            SupportActionBar.Title = ViewModel.Title;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        private void InitializeMembersRecyclerView()
        {
            _membersRecyclerView.HasFixedSize = true;
            _membersRecyclerView.SetLayoutManager(new GuardedLinearLayoutManager(this));
            _membersRecyclerView.AddItemDecoration(new DividerItemDecoration(this, DividerItemDecoration.Vertical));
            _membersRecyclerView.SetAdapter(new BaseChatObservableRecyclerViewAdapter<ChatUserViewModel>(
                ViewModel.Members,
                x =>
                {
                    var itemView = LayoutInflater.From(x.Item1.Context)
                                                 .Inflate(Resource.Layout.item_contact, x.Item1, false);

                    var viewHolder = new ChatUserViewHolder(itemView);
                    viewHolder.ContactSwitch.Visibility = ViewStates.Gone;
                    return viewHolder;
                }));
        }

        private void OpenImagePicker()
        {
            _imagePicker.OpenGallery();
        }

        private bool RebuildMenu(IMenu menu)
        {
            if (_previewImageKey != null)
            {
                _toolbarMenuComponent.BuildMenu(ViewModel.MenuActions, menu);
            }

            return base.OnCreateOptionsMenu(menu);
        }

        private async void OnSaveClick()
        {
            await ViewModel.SaveAsync(_imagePicker.GetStreamFunc()).ConfigureAwait(false);
            Execute.BeginOnUIThread(() => 
            {
                _previewImageKey = null;
                InvalidateOptionsMenu();
            });
        }
    }
}
