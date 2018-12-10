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
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Services;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity]
    public class SelectContactsActivity : ActivityBase<SelectContactsViewModel>
    {
        private NavigationBarView _navigationBarView;
        private RelativeLayout _chatHeaderLayout;
        private MvxCachedImageView _chatPhotoImageView;
        private MvxCachedImageView _chatEditedPhotoImageView;
        private EditText _chatNameEditTextView;
        private RecyclerView _contactsRecyclerView;
        private TextView _membersCountTextView;
        private ImagePicker _imagePicker;
        private string _previewImageKey;
        private Button _changeChatPhotoButton;
        private Button _addMembers;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            OverridePendingTransition(0, 0);

            SetTheme(StyleHelper.Style.CommonActivityStyle);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat_create);

            _navigationBarView = FindViewById<NavigationBarView>(Resource.Id.activity_chat_create_navigation_bar);
            _navigationBarView.SetLeftButton(StyleHelper.Style.NavigationBarBackButtonIcon, ViewModel.BackCommand);
            _navigationBarView.SetTitle(ViewModel.Title);
            _navigationBarView.SetRightButton(ViewModel.ActionButtonName, new RelayCommand(() =>
            {
                KeyboardService.HideSoftKeyboard(_chatNameEditTextView);

                ViewModel.SaveCommand.Execute(_imagePicker.GetStreamFunc());
            }));
            _navigationBarView.RightTextButton.SetBackgroundColor(Color.Transparent);

            _chatHeaderLayout = FindViewById<RelativeLayout>(Resource.Id.rl_chat_create);
            _chatPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);
            _chatEditedPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo_edited);
            _chatNameEditTextView = FindViewById<EditText>(Resource.Id.et_chat_name);
            _contactsRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);
            _membersCountTextView = FindViewById<TextView>(Resource.Id.tv_members_count);
            _changeChatPhotoButton = FindViewById<Button>(Resource.Id.b_chat_change_photo);
            _changeChatPhotoButton.SetCommand(new RelayCommand(ChangePhoto));

            InitializeContactsRecyclerView();

            _imagePicker = new ImagePicker(Dependencies.PermissionsManager, Dependencies.ServiceLocator.Resolve<IImagePickerService>())
            {
                MaxImageWidth = 300
            };

            _chatPhotoImageView.SetImageResource(StyleHelper.Style.ChatGroupNoAvatarIcon);
            _chatEditedPhotoImageView.Visibility = ViewStates.Gone;
            _addMembers = FindViewById<Button>(Resource.Id.activity_chat_create_add_member);
            _addMembers.Text = ViewModel.AddMembersText;
            _addMembers.SetCommand(ViewModel.AddMembersCommand);

            _chatHeaderLayout.Visibility = ViewStates.Visible;
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ActionButtonName, () => SupportActionBar.Title));
            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatNameEditTextView.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.ContactsCountText, () => _membersCountTextView.Text));
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
