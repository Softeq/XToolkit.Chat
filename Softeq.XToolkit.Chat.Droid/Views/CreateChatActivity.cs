// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Cross;
using FFImageLoading.Transformations;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Adapters;
using Softeq.XToolkit.Chat.Droid.LayoutManagers;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Commands;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Services;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity]
    public class CreateChatActivity : ActivityBase<CreateChatViewModel>
    {
        private NavigationBarView _navigationBarView;
        private MvxCachedImageView _chatPhotoImageView;
        private MvxCachedImageView _chatEditedPhotoImageView;
        private EditText _chatNameEditTextView;
        private RecyclerView _contactsRecyclerView;
        private TextView _membersCountTextView;
        private ImagePicker _imagePicker;
        private string _previewImageKey;
        private Button _changeChatPhotoButton;
        private Button _addMembers;
        private BusyOverlayView _busyOverlayView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            OverridePendingTransition(0, 0);

            SetTheme(StyleHelper.Style.CommonActivityStyle);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat_create);

            _navigationBarView = FindViewById<NavigationBarView>(Resource.Id.activity_chat_create_navigation_bar);
            _navigationBarView.SetLeftButton(StyleHelper.Style.NavigationBarBackButtonIcon, ViewModel.BackCommand);
            _navigationBarView.SetTitle(ViewModel.LocalizedStrings.CreateGroup);
            _navigationBarView.SetRightButton(ViewModel.LocalizedStrings.Create, new RelayCommand(() =>
            {
                KeyboardService.HideSoftKeyboard(_chatNameEditTextView);

                ViewModel.SaveCommand.Execute(_imagePicker.GetStreamFunc());
            }));
            _navigationBarView.RightTextButton.SetBackgroundColor(Color.Transparent);

            _chatPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);
            _chatEditedPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo_edited);
            _chatNameEditTextView = FindViewById<EditText>(Resource.Id.et_chat_name);
            _contactsRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);
            _membersCountTextView = FindViewById<TextView>(Resource.Id.tv_members_count);
            _changeChatPhotoButton = FindViewById<Button>(Resource.Id.b_chat_change_photo);
            _changeChatPhotoButton.SetCommand(new RelayCommand(ChangePhoto));
            _changeChatPhotoButton.Text = ViewModel.LocalizedStrings.ChangePhoto;

            InitializeContactsRecyclerView();

            _imagePicker = new ImagePicker(Dependencies.PermissionsManager, Dependencies.Container.Resolve<IImagePickerService>())
            {
                MaxImageWidth = 300
            };

            _chatPhotoImageView.SetImageResource(StyleHelper.Style.ChatGroupNoAvatarIcon);
            _chatEditedPhotoImageView.Visibility = ViewStates.Gone;

            _addMembers = FindViewById<Button>(Resource.Id.activity_chat_create_add_member);
            _addMembers.Text = ViewModel.LocalizedStrings.AddMembers;
            _addMembers.SetCommand(ViewModel.AddMembersCommand);

            _chatNameEditTextView.Hint = ViewModel.LocalizedStrings.ChatName;

            _busyOverlayView = FindViewById<BusyOverlayView>(Resource.Id.activity_chat_create_busy_view);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatNameEditTextView.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.ContactsCountText, () => _membersCountTextView.Text));
            Bindings.Add(this.SetBinding(() => _imagePicker.ViewModel.ImageCacheKey).WhenSourceChanges(() =>
            {
                var newImageCacheKey = _imagePicker.ViewModel.ImageCacheKey;

                if (string.IsNullOrEmpty(newImageCacheKey) || newImageCacheKey == _previewImageKey)
                {
                    return;
                }

                _previewImageKey = newImageCacheKey;

                Execute.BeginOnUIThread(() =>
                {
                    _chatEditedPhotoImageView.Visibility = ViewStates.Visible;
                });

                ImageService.Instance
                    .LoadFile(_previewImageKey)
                    .DownSampleInDip(95, 95)
                    .Transform(new CircleTransformation())
                    .IntoAsync(_chatEditedPhotoImageView);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.IsBusy, () => _busyOverlayView.Visibility)
                .ConvertSourceToTarget(BoolToViewStateConverter.ConvertGone));
        }

        protected override void OnDestroy()
        {
            _contactsRecyclerView.GetAdapter().Dispose();

            base.OnDestroy();
        }

        private void InitializeContactsRecyclerView()
        {
            _contactsRecyclerView.HasFixedSize = true;
            _contactsRecyclerView.SetLayoutManager(new GuardedLinearLayoutManager(this));
            _contactsRecyclerView.AddItemDecoration(new DividerItemDecoration(this, DividerItemDecoration.Vertical));
            _contactsRecyclerView.SetAdapter(new BaseChatObservableRecyclerViewAdapter<ChatUserViewModel>(
                ViewModel.Contacts,
                x =>
                {
                    var itemView = LayoutInflater.From(x.Item1.Context)
                                                 .Inflate(Resource.Layout.item_chat_contact, x.Item1, false);
                    return new ChatUserViewHolder(itemView);
                }));
        }

        private void ChangePhoto()
        {
            _imagePicker.OpenGallery();
        }
    }
}
