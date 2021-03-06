﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Widget;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Adapters;
using Softeq.XToolkit.Chat.Droid.Controls;
using Softeq.XToolkit.Chat.Droid.LayoutManagers;
using Softeq.XToolkit.Chat.Droid.Listeners;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Commands;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity]
    public class ChatMessagesActivity : ActivityBase<ChatMessagesViewModel>
    {
        private NavigationBarView _navigationBarView;
        private RecyclerView _conversationsRecyclerView;
        private ConversationsObservableRecyclerViewAdapter _conversationsAdapter;
        private ImageButton _scrollDownImageButton;
        private ChatInputView _chatInputView;
        private ContextMenuHandler<ChatMessageViewModel> _contextMenuHandler;
        //private bool _shouldSendStateMessageToChat;
        private bool _isAdapterSourceInitialized;
        private bool _isAutoScrollToFooterEnabled = true;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            OverridePendingTransition(0, 0);

            SetTheme(StyleHelper.Style.CommonActivityStyle);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat_conversations);

            _navigationBarView = FindViewById<NavigationBarView>(Resource.Id.activity_chat_conversations_navigation_bar);
            _navigationBarView.SetLeftButton(StyleHelper.Style.NavigationBarBackButtonIcon, ViewModel.BackCommand);
            if (ViewModel.HasInfo)
            {
                _navigationBarView.SetRightButton(StyleHelper.Style.NavigationBarDetailsButtonIcon, ViewModel.ShowInfoCommand);
            }

            _conversationsRecyclerView = FindViewById<RecyclerView>(Resource.Id.rv_conversations_list);

            _scrollDownImageButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_scroll_down);
            _scrollDownImageButton.SetImageResource(StyleHelper.Style.ScrollDownButtonIcon);

            _chatInputView = FindViewById<ChatInputView>(Resource.Id.ll_chat_input_view);

            InitializeConversationsRecyclerView();

            _contextMenuHandler = new ContextMenuHandler<ChatMessageViewModel>(itemViewModel =>
            {
                var commandActions = ViewModel.GetCommandActionsForMessage(itemViewModel);
                return new ContextMenuComponent(commandActions);
            });

            _scrollDownImageButton.SetCommand(nameof(_scrollDownImageButton.Click), new RelayCommand(ScrollToBottom));

            ViewModel.MessageAddedCommand = new RelayCommand(ScrollToBottomIfNeeded);
        }

        protected override void OnPause()
        {
            //if (_shouldSendStateMessageToChat)
            //{
            //    Messenger.Default.Send(new ChatInBackgroundMessage());
            //}
            _chatInputView.HideKeyboard();

            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // TODO YP: think about disconnecting when app in background
            //if (_shouldSendStateMessageToChat)
            //{
            //    Messenger.Default.Send(new ChatInForegroundMessage());
            //}
            //else
            //{
            //    _shouldSendStateMessageToChat = true;
            //}
        }

        //public override void OnBackPressed()
        //{
        //    _shouldSendStateMessageToChat = false;

        //    base.OnBackPressed();
        //}

        protected override void OnDestroy()
        {
            _conversationsRecyclerView.GetAdapter().Dispose();
            _conversationsRecyclerView.ClearOnScrollListeners();

            base.OnDestroy();
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ConnectionStatus.ConnectionStatusText).WhenSourceChanges(() =>
            {
                Execute.BeginOnUIThread(() =>
                {
                    _navigationBarView.SetTitle(ViewModel.ConnectionStatus.ConnectionStatusText);
                });
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.MessagesList.Messages).WhenSourceChanges(() =>
            {
                if (_isAdapterSourceInitialized)
                {
                    return;
                }

                _conversationsAdapter = new ConversationsObservableRecyclerViewAdapter(
                        ViewModel.MessagesList.Messages,
                        ScrollToPositionIfNeeded,
                        LoadItemsRequestedScrollChangeHandler,
                        ViewModel.GetDateString,
                        _contextMenuHandler);

                _conversationsAdapter.SetCommand(nameof(_conversationsAdapter.LastItemRequested),
                    ViewModel.MessagesList.LoadOlderMessagesCommand);

                _conversationsRecyclerView.SetAdapter(_conversationsAdapter);

                _isAdapterSourceInitialized = true;
            }));

            _chatInputView.BindViewModel(ViewModel.MessageInput);
        }

        protected override void DoDetachBindings()
        {
            base.DoDetachBindings();

            _chatInputView.UnbindViewModel();
        }

        private void InitializeConversationsRecyclerView()
        {
            _conversationsRecyclerView.SetLayoutManager(
                new GuardedLinearLayoutManager(this)
                {
                    StackFromEnd = true
                });

            _conversationsRecyclerView.AddOnScrollListener(
                new ScrollDownScrollListener(new RelayCommand<bool>(ScrollDownButtonVisibilityHandler)));
            _conversationsRecyclerView.SetOnTouchListener(
                new RecyclerViewTouchListener(new RelayCommand(() => _chatInputView.HideKeyboard())));
        }

        private void ScrollDownButtonVisibilityHandler(bool isVisible)
        {
            _isAutoScrollToFooterEnabled = !isVisible;
            _scrollDownImageButton.Visibility = BoolToViewStateConverter.ConvertGone(isVisible);
        }

        private void ScrollToBottomIfNeeded()
        {
            ScrollToPositionIfNeeded(_conversationsRecyclerView.GetAdapter().ItemCount - 1);
        }

        private void ScrollToBottom()
        {
            ScrollToPosition(_conversationsRecyclerView.GetAdapter().ItemCount - 1);
        }

        private void ScrollToPositionIfNeeded(int position)
        {
            if (_isAutoScrollToFooterEnabled)
            {
                ScrollToPosition(position);
            }
        }

        private void ScrollToPosition(int lastPosition)
        {
            Execute.OnUIThread(() =>
            {
                _conversationsRecyclerView.ScrollToPosition(lastPosition);
            });
        }

        private void LoadItemsRequestedScrollChangeHandler(int newItemsCount)
        {
            if (_conversationsRecyclerView.GetLayoutManager() is LinearLayoutManager linearLayout)
            {
                var firstVisiblePosition = linearLayout.FindLastVisibleItemPosition();

                var scrollPosition = newItemsCount + firstVisiblePosition - 1;

                Execute.OnUIThread(() =>
                {
                    _conversationsRecyclerView.ScrollToPosition(scrollPosition);
                });
            }
        }
    }
}
