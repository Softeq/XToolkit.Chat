// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Adapters;
using Softeq.XToolkit.Chat.Droid.Controls;
using Softeq.XToolkit.Chat.Droid.LayoutManagers;
using Softeq.XToolkit.Chat.Droid.Listeners;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Services;
using Softeq.XToolkit.WhiteLabel.Threading;
using AndroidResource = Android.Resource;
using Messenger = Softeq.XToolkit.WhiteLabel.Messenger.Messenger;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    public class ChatMessagesFragment : FragmentBase<ChatMessagesViewModel>
    {
        private RecyclerView _conversationsRecyclerView;
        private ConversationsObservableRecyclerViewAdapter _conversationsAdapter;
        private EditText _messageEditText;
        private ImageButton _addAttachmentButton;
        private ImageButton _sendButton;
        private RelativeLayout _editingMessageLayout;
        private TextView _editingMessageBodyTextView;
        private ImageButton _editingMessageCloseButton;
        private ImageButton _scrollDownImageButton;
        private ContextMenuComponent _contextMenuComponent;
        private bool _shouldSendStateMessageToChat;
        private bool _isAdapterSourceInitialized;
        private bool _isAutoScrollToFooterEnabled = true;

        private AppCompatActivity SupportActivity => (AppCompatActivity)Activity;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.activity_conversations, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar_conversations);

            _conversationsRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.rv_conversations_list);
            _messageEditText = view.FindViewById<EditText>(Resource.Id.et_conversations_message);
            _addAttachmentButton = view.FindViewById<ImageButton>(Resource.Id.ib_conversations_add_attachment);
            _sendButton = view.FindViewById<ImageButton>(Resource.Id.ib_conversations_send);
            _editingMessageLayout = view.FindViewById<RelativeLayout>(Resource.Id.rl_conversations_editing_message);
            _editingMessageBodyTextView = view.FindViewById<TextView>(Resource.Id.tv_editing_message_body);
            _editingMessageCloseButton = view.FindViewById<ImageButton>(Resource.Id.ib_conversations_editing_message_close);
            _scrollDownImageButton = view.FindViewById<ImageButton>(Resource.Id.ib_conversations_scroll_down);

            InitializeToolbar(toolbar);
            InitializeConversationsRecyclerView();

            _contextMenuComponent = new ContextMenuComponent(ViewModel.MessageCommandActions);

            _sendButton.SetCommand(nameof(_sendButton.Click), ViewModel.SendCommand);
            _editingMessageCloseButton.SetCommand(nameof(_editingMessageCloseButton.Click), ViewModel.CancelEditingMessageModeCommand);
            _scrollDownImageButton.SetCommand(nameof(_scrollDownImageButton.Click), new RelayCommand(() =>
            {
                ScrollToPosition(_conversationsRecyclerView.GetAdapter().ItemCount - 1);
            }));
            //_addAttachmentButton.SetCommand(nameof(_addAttachmentButton.Click), ViewModel.AttachImageCommand);
        }

        public override void OnPause()
        {
            if (_shouldSendStateMessageToChat)
            {
                Messenger.Default.Send(new ChatInBackgroundMessage());
            }

            KeyboardService.HideSoftKeyboard(_messageEditText);

            base.OnPause();
        }

        public override void OnResume()
        {
            base.OnResume();

            if (_shouldSendStateMessageToChat)
            {
                Messenger.Default.Send(new ChatInForegroundMessage());
            }
            else
            {
                _shouldSendStateMessageToChat = true;
            }
        }

        //public override void OnBackPressed()
        //{
        //    _shouldSendStateMessageToChat = false;

        //    base.OnBackPressed();
        //}

        public override void OnDestroy()
        {
            _conversationsRecyclerView.GetAdapter().Dispose();
            _conversationsRecyclerView.ClearOnScrollListeners();
            // TODO: remove onTouchListener

            base.OnDestroy();
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ConnectionStatusViewModel.ConnectionStatusText).WhenSourceChanges(() =>
            {
                Execute.BeginOnUIThread(() =>
                {
                    SupportActivity.SupportActionBar.Title = ViewModel.ConnectionStatusViewModel.ConnectionStatusText;
                });
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.MessageToSendBody, () => _messageEditText.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.IsInEditMessageMode).WhenSourceChanges(() =>
            {
                if (ViewModel.IsInEditMessageMode)
                {
                    _editingMessageBodyTextView.Text = ViewModel.EditedMessageOriginalBody;

                    KeyboardService.ShowSoftKeyboard(_messageEditText);
                }
                else
                {
                    KeyboardService.HideSoftKeyboard(_messageEditText);
                }

                _editingMessageLayout.Visibility = BoolToViewStateConverter.ConvertGone(ViewModel.IsInEditMessageMode);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.Messages).WhenSourceChanges(() =>
            {
                if (_isAdapterSourceInitialized)
                {
                    return;
                }

                _conversationsAdapter = new ConversationsObservableRecyclerViewAdapter(
                        ViewModel.Messages,
                        CollectionChangedAutoScrollToBottomHandler,
                        LoadItemsRequestedScrollChangeHandler,
                        ViewModel.GetDateString,
                        _contextMenuComponent);

                _conversationsAdapter.SetCommand(nameof(_conversationsAdapter.LastItemRequested), ViewModel.LoadOlderMessagesCommand);

                _conversationsRecyclerView.SetAdapter(_conversationsAdapter);

                _isAdapterSourceInitialized = true;
            }));
        }

        private void ScrollToPosition(int lastPosition)
        {
            Execute.OnUIThread(() =>
            {
                _conversationsRecyclerView.ScrollToPosition(lastPosition);
            });
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.toolbar_conversations, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == AndroidResource.Id.Home)
            {
                ViewModel.BackCommand.Execute(null);
                return true;
            }
            if (item.ItemId == Resource.Id.action_details)
            {
                ViewModel.ShowInfoCommand.Execute(null);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void InitializeToolbar(Toolbar toolbar)
        {
            SupportActivity.SetSupportActionBar(toolbar);

            SupportActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            HasOptionsMenu = true;
        }

        private void InitializeConversationsRecyclerView()
        {
            _conversationsRecyclerView.SetLayoutManager(
                new GuardedLinearLayoutManager(Activity)
                {
                    StackFromEnd = true
                });

            _conversationsRecyclerView.AddOnScrollListener(
                new ScrollDownScrollListener(new RelayCommand<bool>(ScrollDownButtonVisibilityHandler)));
            _conversationsRecyclerView.SetOnTouchListener(
                new RecyclerViewTouchListener(new RelayCommand(() => KeyboardService.HideSoftKeyboard(_messageEditText))));
        }

        private void ScrollDownButtonVisibilityHandler(bool isVisible)
        {
            _isAutoScrollToFooterEnabled = !isVisible;
            _scrollDownImageButton.Visibility = BoolToViewStateConverter.ConvertGone(isVisible);
        }

        private void CollectionChangedAutoScrollToBottomHandler(int bottomPosition)
        {
            if (_isAutoScrollToFooterEnabled)
            {
                ScrollToPosition(bottomPosition);
            }
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
