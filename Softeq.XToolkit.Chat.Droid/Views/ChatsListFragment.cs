// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Adapters;
using Softeq.XToolkit.Chat.Droid.Controls;
using Softeq.XToolkit.Chat.Droid.ItemTouchCallbacks;
using Softeq.XToolkit.Chat.Droid.LayoutManagers;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Droid.Extensions;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    public class ChatsListFragment : FragmentBase<ChatsListViewModel>
    {
        private NavigationBarView _navigationBarView;
        private RecyclerView _chatsRecyclerView;
        private FloatingActionButton _createChatFloatButton;
        private SimpleSwipeActionView.Options _swipeLeaveActionViewOptions;
        private SimpleSwipeActionView.Options _swipeCloseActionViewOptions;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_chat_list, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            _navigationBarView = view.FindViewById<NavigationBarView>(Resource.Id.fragment_chats_navigation_bar);

            if (StyleHelper.Style.UseLogoInsteadOfConnectionStatus)
            {
                _navigationBarView.SetCenterImage(StyleHelper.Style.LogoIcon, null);
            }

            _createChatFloatButton = view.FindViewById<FloatingActionButton>(Resource.Id.fab_create_chat);
            _chatsRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.rv_chats_list);

            InitializeRecyclerView();

            _createChatFloatButton.SetCommand(nameof(_createChatFloatButton.Click), ViewModel.CreateChatCommand);

            _swipeLeaveActionViewOptions = new SimpleSwipeActionView.Options
            {
                Width = Activity.ToPixels(80),
                TextSize = Activity.ToPixels(14),
                BackgroundColor = Color.Red
            };
            _swipeCloseActionViewOptions = new SimpleSwipeActionView.Options
            {
                Width = Activity.ToPixels(80),
                TextSize = Activity.ToPixels(14),
                BackgroundColor = Color.Orange
            };
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            if (!StyleHelper.Style.UseLogoInsteadOfConnectionStatus)
            {
                Bindings.Add(this.SetBinding(() => ViewModel.ConnectionStatusViewModel.ConnectionStatusText).WhenSourceChanges(() =>
                {
                    Execute.BeginOnUIThread(() =>
                    {
                        _navigationBarView.SetTitle(ViewModel.ConnectionStatusViewModel.ConnectionStatusText);
                    });
                }));
            }
        }

        public override void OnDestroy()
        {
            _chatsRecyclerView.GetAdapter().Dispose();

            base.OnDestroy();
        }

        private void InitializeRecyclerView()
        {
            _chatsRecyclerView.HasFixedSize = true;
            _chatsRecyclerView.SetLayoutManager(new GuardedLinearLayoutManager(Activity));
            _chatsRecyclerView.AddItemDecoration(new LeftOffsetItemDecoration(Activity, Resource.Color.chat_divider_color, 72));
            _chatsRecyclerView.SetAdapter(new ChatObservableRecyclerViewAdapter(ViewModel.Chats,
                CreateChatViewHolder, _chatsRecyclerView.SmoothScrollToPosition));

            var swipeItemCallback = new SwipeCallback(Activity, _chatsRecyclerView, ConfigureSwipeForViewHolder);
            var swipeItemTouchHelper = new ItemTouchHelper(swipeItemCallback);
            swipeItemTouchHelper.AttachToRecyclerView(_chatsRecyclerView);
        }

        private BindableViewHolder<ChatSummaryViewModel> CreateChatViewHolder((ViewGroup parent, int viewType) options)
        {
            var itemView = LayoutInflater.From(options.parent.Context).Inflate(Resource.Layout.item_chat_list, options.parent, false);
            return new ChatViewHolder(itemView, selectedViewModel => ViewModel.SelectedChat = selectedViewModel);
        }

        private void ConfigureSwipeForViewHolder(RecyclerView.ViewHolder viewHolder, int position,
                                                 ICollection<SwipeCallback.ISwipeActionView> actions)
        {
            if (ViewModel.Chats[position].CanLeave)
            {
                actions.Add(new SimpleSwipeActionView(ViewModel.LocalizedStrings.Leave, _swipeLeaveActionViewOptions, pos =>
                {
                    ViewModel.LeaveChatCommand.Execute(ViewModel.Chats[pos]);
                }));
            }

            if (ViewModel.Chats[position].CanClose)
            {
                actions.Add(new SimpleSwipeActionView(ViewModel.LocalizedStrings.Delete, _swipeCloseActionViewOptions, pos =>
                {
                    ViewModel.DeleteChatCommand.Execute(ViewModel.Chats[pos]);
                }));
            }
        }
    }
}
