// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.App;
using Android.OS;
using Android.Support.V7.App;
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
using AndroidResource = Android.Resource;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    public class SelectContactsFragment : FragmentBase<SelectContactsViewModel>
    {
        private const string TempChatPhotoUrl = "https://cdn.pixabay.com/photo/2015/10/23/17/03/eye-1003315_960_720.jpg";

        private RelativeLayout _chatHeaderLayout;
        private MvxCachedImageView _chatPhotoImageView;
        private EditText _chatNameEditTextView;
        private RecyclerView _contactsRecyclerView;
        private TextView _membersCountTextView;

        private AppCompatActivity SupportActivity => (AppCompatActivity)Activity;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_chat_create, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar_chat_create);

            _chatHeaderLayout = view.FindViewById<RelativeLayout>(Resource.Id.rl_chat_create);
            _chatPhotoImageView = view.FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);
            _chatNameEditTextView = view.FindViewById<EditText>(Resource.Id.et_chat_name);
            _contactsRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);
            _membersCountTextView = view.FindViewById<TextView>(Resource.Id.tv_members_count);

            InitializeToolbar(toolbar);
            InitializeContactsRecyclerView();

            ImageService.Instance
                        .LoadUrl(TempChatPhotoUrl)
                        .Transform(new CircleTransformation())
                        .IntoAsync(_chatPhotoImageView);
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.toolbar_create_chat, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == AndroidResource.Id.Home)
            {
                ViewModel.BackCommand.Execute(null);
                return true;
            }
            if (item.ItemId == Resource.Id.toolbar_chat_create_action)
            {
                ViewModel.AddChatCommand.Execute(null);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitializeToolbar(Toolbar toolbar)
        {
            SupportActivity.SetSupportActionBar(toolbar);
            SupportActivity.SupportActionBar.Title = string.Empty;
            SupportActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            HasOptionsMenu = true;
        }

        private void InitializeContactsRecyclerView()
        {
            _contactsRecyclerView.HasFixedSize = true;
            _contactsRecyclerView.SetLayoutManager(new GuardedLinearLayoutManager(Activity));
            _contactsRecyclerView.AddItemDecoration(new DividerItemDecoration(Activity, DividerItemDecoration.Vertical));
            _contactsRecyclerView.SetAdapter(new BaseChatObservableRecyclerViewAdapter<ChatUserViewModel>(
                ViewModel.Contacts,
                x =>
                {
                    var itemView = LayoutInflater.From(x.Item1.Context)
                                                 .Inflate(Resource.Layout.item_contact, x.Item1, false);
                    return new ChatUserViewHolder(itemView);
                }));
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ActionButtonName, () => SupportActivity.SupportActionBar.Title));
            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatNameEditTextView.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.ContactsCountText, () => _membersCountTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.IsCreateChat).WhenSourceChanges(() =>
            {
                _chatHeaderLayout.Visibility = BoolToViewStateConverter.ConvertGone(ViewModel.IsCreateChat);
            }));
        }

        public override void OnDestroy()
        {
            _contactsRecyclerView.GetAdapter().Dispose();

            base.OnDestroy();
        }
    }
}
