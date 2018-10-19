﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Adapters;
using Softeq.XToolkit.Chat.Droid.ItemTouchCallbacks;
using Softeq.XToolkit.Chat.Droid.LayoutManagers;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Droid.Extensions;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity(Theme = "@style/ChatTheme")]
    public class ChatsListActivity : ActivityBase<ChatsListViewModel>
    {
        private RecyclerView _chatsRecyclerView;
        private FloatingActionButton _createChatFloatButton;
        private SimpleSwipeActionView.Options _swipeLeaveActionViewOptions;
        private SimpleSwipeActionView.Options _swipeCloseActionViewOptions;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // navigated from start/conversations <-
            OverridePendingTransition(
                Resource.Animation.news_enter_left_to_right,
                Resource.Animation.news_exit_right_to_left);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar_chat);
            SetSupportActionBar(toolbar);

            _createChatFloatButton = FindViewById<FloatingActionButton>(Resource.Id.fab_create_chat);
            _chatsRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_chats_list);

            InitializeRecyclerView();

            _createChatFloatButton.SetCommand(nameof(_createChatFloatButton.Click), ViewModel.CreateChatCommand);

            _swipeLeaveActionViewOptions = new SimpleSwipeActionView.Options
            {
                Width = this.ToPixels(80),
                TextSize = this.ToPixels(14),
                BackgroundColor = Color.Red
            };
            _swipeCloseActionViewOptions = new SimpleSwipeActionView.Options
            {
                Width = this.ToPixels(80),
                TextSize = this.ToPixels(14),
                BackgroundColor = Color.Orange
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.toolbar_chat, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.toolbar_chat_action_login)
            {
                ViewModel.LoginCommand.Execute(null);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void Finish()
        {
            base.Finish();

            // navigate to conversations/create_chat ->
            OverridePendingTransition(
                Resource.Animation.news_enter_right_to_left,
                Resource.Animation.news_exit_left_to_right);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ConnectionStatusViewModel.ConnectionStatusText).WhenSourceChanges(() =>
            {
                Execute.BeginOnUIThread(() =>
                {
                    SupportActionBar.Title = ViewModel.ConnectionStatusViewModel.ConnectionStatusText;
                });
            }));
        }

        protected override void OnDestroy()
        {
            _chatsRecyclerView.GetAdapter().Dispose();

            base.OnDestroy();
        }

        private void InitializeRecyclerView()
        {
            _chatsRecyclerView.HasFixedSize = true;
            _chatsRecyclerView.SetLayoutManager(new GuardedLinearLayoutManager(this));
            _chatsRecyclerView.SetAdapter(new ChatObservableRecyclerViewAdapter(ViewModel.Chats,
                CreateChatViewHolder, _chatsRecyclerView.SmoothScrollToPosition));

            var swipeItemCallback = new SwipeCallback(this, _chatsRecyclerView, ConfigureSwipeForViewHolder);
            var swipeItemTouchHelper = new ItemTouchHelper(swipeItemCallback);
            swipeItemTouchHelper.AttachToRecyclerView(_chatsRecyclerView);
        }

        private BindableViewHolder<ChatSummaryViewModel> CreateChatViewHolder((ViewGroup parent, int viewType) options)
        {
            var itemView = LayoutInflater.From(options.parent.Context).Inflate(Resource.Layout.item_chat, options.parent, false);
            return new ChatViewHolder(itemView, selectedViewModel => ViewModel.SelectedChat = selectedViewModel);
        }

        private void ConfigureSwipeForViewHolder(RecyclerView.ViewHolder viewHolder, int position,
                                                 ICollection<SwipeCallback.ISwipeActionView> actions)
        {
            actions.Add(new SimpleSwipeActionView("Leave", _swipeLeaveActionViewOptions, pos =>
            {
                ViewModel.LeaveChatCommand.Execute(ViewModel.Chats[pos]);
            }));

            if (ViewModel.Chats[position].IsCreatedByMe)
            {
                actions.Add(new SimpleSwipeActionView("Close", _swipeCloseActionViewOptions, pos =>
                {
                    ViewModel.DeleteChatCommand.Execute(ViewModel.Chats[pos]);
                }));
            }
        }
    }
}
