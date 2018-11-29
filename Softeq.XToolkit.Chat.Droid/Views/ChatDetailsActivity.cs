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
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity(Theme = "@style/ChatTheme")]
    public class ChatDetailsActivity : ActivityBase<ChatDetailsViewModel>
    {
        private NavigationBarView _navigationBarView;
        private MvxCachedImageView _chatPhotoImageView;
        private TextView _chatNameTextView;
        private TextView _chatMembersCountTextView;
        private Button _addMemberButton;
        private RecyclerView _membersRecyclerView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            OverridePendingTransition(0, 0);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat_details);

            _navigationBarView = FindViewById<NavigationBarView>(Resource.Id.activity_chat_details_navigation_bar);
            _navigationBarView.SetLeftButton(ExternalResourceIds.NavigationBarBackButtonIcon, ViewModel.BackCommand);
            _navigationBarView.SetTitle(ViewModel.Title);

            _chatPhotoImageView = FindViewById<MvxCachedImageView>(Resource.Id.iv_chat_photo);
            _chatNameTextView = FindViewById<TextView>(Resource.Id.tv_chat_name);
            _chatMembersCountTextView = FindViewById<TextView>(Resource.Id.tv_members_count);
            _addMemberButton = FindViewById<Button>(Resource.Id.b_chat_add_member);
            _membersRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_contacts_list);

            InitializeMembersRecyclerView();

            _addMemberButton.SetCommand(ViewModel.AddMembersCommand);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.Summary.Name, () => _chatNameTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.MembersCountText, () => _chatMembersCountTextView.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.Summary.AvatarUrl).WhenSourceChanges(() =>
            {
                ImageService.Instance
                        .LoadUrl(ViewModel.Summary.AvatarUrl)
                        .Transform(new CircleTransformation())
                        .IntoAsync(_chatPhotoImageView);
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
            
            actions.Add(new SimpleSwipeActionView(ViewModel.RemoveLocalizedString, swipeLeaveActionViewOptions,
                pos => { ViewModel.RemoveMemberAtCommand.Execute(pos); }));
        }
    }
}