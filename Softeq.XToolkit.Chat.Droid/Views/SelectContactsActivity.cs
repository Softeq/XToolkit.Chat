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
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity(Theme = "@style/ChatTheme")]
    public class SelectContactsActivity : ActivityBase<SelectContactsViewModel>
    {
        private const string TempChatPhotoUrl = "https://cdn.pixabay.com/photo/2015/10/23/17/03/eye-1003315_960_720.jpg";

        private NavigationBarView _navigationBarView;
        private RelativeLayout _chatHeaderLayout;
        private MvxCachedImageView _chatPhotoImageView;
        private EditText _chatNameEditTextView;
        private RecyclerView _contactsRecyclerView;
        private TextView _membersCountTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            OverridePendingTransition(0, 0);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat_create);

            _navigationBarView = FindViewById<NavigationBarView>(Resource.Id.activity_chat_create_navigation_bar);
            _navigationBarView.SetLeftButton(ExternalResourceIds.NavigationBarBackButtonIcon, ViewModel.BackCommand);
            _navigationBarView.SetTitle(ViewModel.Title);
            _navigationBarView.SetRightButton(ViewModel.ActionButtonName, ViewModel.AddChatCommand);
            _navigationBarView.RightTextButton.SetBackgroundColor(Color.Transparent);

            _chatHeaderLayout = FindViewById<RelativeLayout>(Resource.Id.rl_chat_create);
            _chatPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);
            _chatNameEditTextView = FindViewById<EditText>(Resource.Id.et_chat_name);
            _contactsRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);
            _membersCountTextView = FindViewById<TextView>(Resource.Id.tv_members_count);

            InitializeContactsRecyclerView();

            ImageService.Instance
                        .LoadUrl(TempChatPhotoUrl)
                        .Transform(new CircleTransformation())
                        .IntoAsync(_chatPhotoImageView);
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

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatNameEditTextView.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.ContactsCountText, () => _membersCountTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.IsCreateChat).WhenSourceChanges(() =>
            {
                _chatHeaderLayout.Visibility = BoolToViewStateConverter.ConvertGone(ViewModel.IsCreateChat);
            }));
        }

        protected override void OnDestroy()
        {
            _contactsRecyclerView.GetAdapter().Dispose();

            base.OnDestroy();
        }
    }
}
