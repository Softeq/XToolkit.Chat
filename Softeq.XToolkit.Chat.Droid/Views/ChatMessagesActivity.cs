// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.Droid.Adapters;
using Softeq.XToolkit.Chat.Droid.Controls;
using Softeq.XToolkit.Chat.Droid.LayoutManagers;
using Softeq.XToolkit.Chat.Droid.Listeners;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.Common.EventArguments;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Services;
using Softeq.XToolkit.WhiteLabel.ImagePicker;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity]
    public class ChatMessagesActivity : ActivityBase<ChatMessagesViewModel>
    {
        private NavigationBarView _navigationBarView;
        private RecyclerView _conversationsRecyclerView;
        private ConversationsObservableRecyclerViewAdapter _conversationsAdapter;
        private EditText _messageEditText;
        private ImageButton _takeAttachmentButton;
        private ImageButton _addAttachmentButton;
        private ImageButton _sendButton;
        private RelativeLayout _editingMessageLayout;
        private TextView _editingMessageBodyTextView;
        private ImageButton _editingMessageCloseButton;
        private ImageButton _scrollDownImageButton;
        private ContextMenuComponent _contextMenuComponent;
        //private bool _shouldSendStateMessageToChat;
        private bool _isAdapterSourceInitialized;
        private bool _isAutoScrollToFooterEnabled = true;
        private ImagePicker _imagePicker;
        private View _editImageContainer;
        private ImageButton _removeImageButton;
        private ImageViewAsync _imagePreview;
        private string _previewImageKey;

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

            _messageEditText = FindViewById<EditText>(Resource.Id.et_conversations_message);
            _messageEditText.Hint = ViewModel.MessageInput.EnterMessagePlaceholderString;

            _editingMessageLayout = FindViewById<RelativeLayout>(Resource.Id.rl_conversations_editing_message);
            _editingMessageBodyTextView = FindViewById<TextView>(Resource.Id.tv_editing_message_body);
            _editingMessageCloseButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_editing_message_close);
            _editingMessageCloseButton.SetImageResource(StyleHelper.Style.EditingCloseButtonIcon);

            _scrollDownImageButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_scroll_down);
            _scrollDownImageButton.SetImageResource(StyleHelper.Style.ScrollDownButtonIcon);

            _takeAttachmentButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_take_attachment);
            _takeAttachmentButton.SetImageResource(StyleHelper.Style.TakeAttachmentButtonIcon);
            _takeAttachmentButton.SetCommand(new RelayCommand(TakePhoto));

            _addAttachmentButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_add_attachment);
            _addAttachmentButton.SetImageResource(StyleHelper.Style.AddAttachmentButtonIcon);
            _addAttachmentButton.SetCommand(new RelayCommand(AddPhoto));

            _sendButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_send);
            _sendButton.SetImageResource(StyleHelper.Style.SendMessageButtonIcon);
            _sendButton.SetCommand(nameof(_sendButton.Click), new RelayCommand(Send));

            InitializeConversationsRecyclerView();

            _contextMenuComponent = new ContextMenuComponent(ViewModel.MessageCommandActions);

            _editingMessageCloseButton.SetCommand(nameof(_editingMessageCloseButton.Click), ViewModel.MessageInput.CancelEditingCommand);
            _scrollDownImageButton.SetCommand(nameof(_scrollDownImageButton.Click), new RelayCommand(() =>
            {
                ScrollToPosition(_conversationsRecyclerView.GetAdapter().ItemCount - 1);
            }));

            _imagePicker = new ImagePicker(Dependencies.PermissionsManager, Dependencies.IocContainer.Resolve<IImagePickerService>());

            _editImageContainer = FindViewById<View>(Resource.Id.activity_chat_conversations_image_preview_container);
            _imagePreview = FindViewById<ImageViewAsync>(Resource.Id.activity_chat_conversations_preview_image);
            _removeImageButton = FindViewById<ImageButton>(Resource.Id.activity_chat_conversations_remove_image_button);
            _removeImageButton.SetImageResource(StyleHelper.Style.RemoveImageButtonIcon);
            _removeImageButton.SetCommand(new RelayCommand(RemoveAttachment));
            _editImageContainer.Visibility = ViewStates.Gone;

            var editingMessageHeader = FindViewById<TextView>(Resource.Id.tv_conversations_editing_message_header);
            editingMessageHeader.Text = ViewModel.MessageInput.EditMessageHeaderString;
        }

        protected override void OnPause()
        {
            //if (_shouldSendStateMessageToChat)
            //{
            //    Messenger.Default.Send(new ChatInBackgroundMessage());
            //}

            KeyboardService.HideSoftKeyboard(_messageEditText);

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
            // TODO: remove onTouchListener

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
            Bindings.Add(this.SetBinding(() => ViewModel.MessageInput.MessageBody, () => _messageEditText.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.MessageInput.IsInEditMessageMode).WhenSourceChanges(() =>
            {
                if (ViewModel.MessageInput.IsInEditMessageMode)
                {
                    _editingMessageBodyTextView.Text = ViewModel.MessageInput.EditedMessageOriginalBody;

                    KeyboardService.ShowSoftKeyboard(_messageEditText);
                }
                else
                {
                    KeyboardService.HideSoftKeyboard(_messageEditText);
                }

                _editingMessageLayout.Visibility = BoolToViewStateConverter.ConvertGone(ViewModel.MessageInput.IsInEditMessageMode);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.MessagesList.Messages).WhenSourceChanges(() =>
            {
                if (_isAdapterSourceInitialized)
                {
                    return;
                }

                _conversationsAdapter = new ConversationsObservableRecyclerViewAdapter(
                        ViewModel.MessagesList.Messages,
                        CollectionChangedAutoScrollToBottomHandler,
                        LoadItemsRequestedScrollChangeHandler,
                        ViewModel.GetDateString,
                        _contextMenuComponent);

                _conversationsAdapter.SetCommand(nameof(_conversationsAdapter.LastItemRequested),
                    ViewModel.MessagesList.LoadOlderMessagesCommand);

                _conversationsRecyclerView.SetAdapter(_conversationsAdapter);

                _isAdapterSourceInitialized = true;
            }));

            Bindings.Add(this.SetBinding(() => _imagePicker.ViewModel.ImageCacheKey).WhenSourceChanges(() =>
            {
                if (_imagePicker.ViewModel.ImageCacheKey == null)
                {
                    CloseEditImageContainer();
                    return;
                }

                OpenEditImageContainer();
            }));
        }

        private void ScrollToPosition(int lastPosition)
        {
            Execute.OnUIThread(() =>
            {
                _conversationsRecyclerView.ScrollToPosition(lastPosition);
            });
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

        private void TakePhoto()
        {
            _imagePicker.OpenCamera();
        }

        private void AddPhoto()
        {
            _imagePicker.OpenGallery();
        }

        private void OpenEditImageContainer()
        {
            Execute.BeginOnUIThread(() =>
            {
                var key = _imagePicker.ViewModel.ImageCacheKey;
                if (key == _previewImageKey)
                {
                    return;
                }

                _editImageContainer.Visibility = ViewStates.Visible;

                _previewImageKey = key;

                ImageService.Instance
                    .LoadFile(key)
                    .DownSampleInDip(60, 60)
                    .IntoAsync(_imagePreview);
            });
        }

        private void CloseEditImageContainer()
        {
            Execute.BeginOnUIThread(() =>
            {
                _editImageContainer.Visibility = ViewStates.Gone;
                _previewImageKey = null;
                _imagePicker.ViewModel.ImageCacheKey = null;
                _imagePreview.SetImageDrawable(null);
            });
        }

        private void RemoveAttachment()
        {
            _imagePicker.ViewModel.ImageCacheKey = null;
            _previewImageKey = null;
        }

        private void Send()
        {
            var args = _imagePicker.GetPickerData();
            ViewModel.MessageInput.SendMessageCommand.Execute(new GenericEventArgs<ImagePickerArgs>(args));
            CloseEditImageContainer();
        }
    }
}
