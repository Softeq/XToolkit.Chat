// Developed by Softeq Development Corporation
// http://www.softeq.com

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
using Softeq.XToolkit.WhiteLabel.Droid;
using AndroidResource = Android.Resource;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    public class ChatDetailsFragment : FragmentBase<ChatDetailsViewModel>
    {
        private MvxCachedImageView _chatPhotoImageView;
        private TextView _chatNameTextView;
        private TextView _chatMembersCountTextView;
        private Button _addMemberButton;
        private RecyclerView _membersRecyclerView;
        //private bool _isNavigateBack;

        private AppCompatActivity SupportActivity => (AppCompatActivity)Activity;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_chat_details, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar_chat_details);

            _chatPhotoImageView = view.FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);
            _chatNameTextView = view.FindViewById<TextView>(Resource.Id.tv_chat_name);
            _chatMembersCountTextView = view.FindViewById<TextView>(Resource.Id.tv_members_count);
            _addMemberButton = view.FindViewById<Button>(Resource.Id.b_chat_add_member);
            _membersRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);

            InitializeToolbar(toolbar);
            InitializeMembersRecyclerView();

            _addMemberButton.SetCommand(ViewModel.AddMembersCommand);
        }

        //protected override void OnCreate(Bundle savedInstanceState)
        //{
        //    //// FIXME:
        //    //if (ViewModel.IsNavigated)
        //    //{
        //    //    // navigated from conversations ->
        //    //    OverridePendingTransition(
        //    //        Resource.Animation.news_enter_right_to_left,
        //    //        Resource.Animation.news_exit_left_to_right);
        //    //}
        //    //else
        //    //{
        //    //    // navigated from add members <-
        //    //    OverridePendingTransition(
        //    //        Resource.Animation.news_enter_left_to_right,
        //    //        Resource.Animation.news_exit_right_to_left);
        //    //}
        //}

        //public override void OnBackPressed()
        //{
        //    _isNavigateBack = true;
        //    base.OnBackPressed();
        //}

        //public override void Finish()
        //{
        //    base.Finish();
        //    // FIXME:
        //    if (_isNavigateBack)
        //    {
        //        // navigate to conversations <-
        //        OverridePendingTransition(
        //            Resource.Animation.news_enter_left_to_right,
        //            Resource.Animation.news_exit_right_to_left);
        //    }
        //    else
        //    {
        //        // navigate to add members ->
        //        OverridePendingTransition(
        //            Resource.Animation.news_enter_right_to_left,
        //            Resource.Animation.news_exit_left_to_right);
        //    }
        //}

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatNameTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.MembersCountText, () => _chatMembersCountTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.ChatAvatarUrl).WhenSourceChanges(() =>
            {
                ImageService.Instance
                        .LoadUrl(ViewModel.ChatAvatarUrl)
                        .Transform(new CircleTransformation())
                        .IntoAsync(_chatPhotoImageView);
            }));
        }

        public override void OnDestroy()
        {
            _membersRecyclerView.GetAdapter().Dispose();

            base.OnDestroy();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == AndroidResource.Id.Home)
            {
                ViewModel.BackCommand.Execute(null);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitializeToolbar(Toolbar toolbar)
        {
            SupportActivity.SetSupportActionBar(toolbar);
            SupportActivity.SupportActionBar.Title = ViewModel.Title;
            SupportActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            HasOptionsMenu = true;
        }

        private void InitializeMembersRecyclerView()
        {
            _membersRecyclerView.HasFixedSize = true;
            _membersRecyclerView.SetLayoutManager(new GuardedLinearLayoutManager(Activity));
            _membersRecyclerView.AddItemDecoration(new DividerItemDecoration(Activity, DividerItemDecoration.Vertical));
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
    }
}
