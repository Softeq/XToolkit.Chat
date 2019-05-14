
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using FFImageLoading.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.Droid.Services;
using Softeq.XToolkit.Bindings.Extensions;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Threading;
using FFImageLoading;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.Common.EventArguments;
using Softeq.XToolkit.WhiteLabel.ImagePicker;
using Softeq.XToolkit.Common.Command;

namespace Softeq.XToolkit.Chat.Droid.Controls
{
    [Register("com.softeq.xtoolkit.chat.droid.ChatInputView")]
    public class ChatInputView : LinearLayout
    {
        private WeakReferenceEx<ChatMessageInputViewModel> _viewModelRef;
        private List<Binding> _bindings;
        private EditText _messageEditText;
        private ImageButton _editingMessageCloseButton;
        private ImageButton _takeAttachmentButton;
        private ImageButton _addAttachmentButton;
        private ImageButton _sendButton;
        private ImageButton _removeImageButton;
        private View _editImageContainer;
        private TextView _editingMessageHeader;
        private TextView _editingMessageBodyTextView;
        private RelativeLayout _editingMessageLayout;
        private ImageViewAsync _imagePreview;
        private ImagePicker _imagePicker;
        private string _previewImageKey;

        public ChatInputView(Context context) :
            base(context)
        {
            Initialize(context);
        }

        public ChatInputView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context);
        }

        public ChatInputView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context);
        }

        public void BindViewModel(ChatMessageInputViewModel viewModel)
        {
            _viewModelRef = new WeakReferenceEx<ChatMessageInputViewModel>(viewModel);

            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.MessageBody, () => _messageEditText.Text, BindingMode.TwoWay));
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.IsInEditMessageMode).WhenSourceChanges(() =>
            {
                if (_viewModelRef.Target.IsInEditMessageMode)
                {
                    _editingMessageBodyTextView.Text = _viewModelRef.Target.EditedMessageOriginalBody;

                    KeyboardService.ShowSoftKeyboard(_messageEditText);
                }
                else
                {
                    KeyboardService.HideSoftKeyboard(_messageEditText);
                }

                _editingMessageLayout.Visibility = BoolToViewStateConverter.ConvertGone(_viewModelRef.Target.IsInEditMessageMode);
            }));
            _bindings.Add(this.SetBinding(() => _imagePicker.ViewModel.ImageCacheKey).WhenSourceChanges(() =>
            {
                if (_imagePicker.ViewModel.ImageCacheKey == null)
                {
                    CloseEditImageContainer();
                    return;
                }

                OpenEditImageContainer();
            }));

            _editingMessageCloseButton.SetCommand(nameof(_editingMessageCloseButton.Click), _viewModelRef.Target.CancelEditingCommand);


            _messageEditText.Hint = _viewModelRef.Target.EnterMessagePlaceholderString;
            _editingMessageHeader.Text = _viewModelRef.Target.EditMessageHeaderString;
        }

        public void UnbindViewModel()
        {
            _bindings.DetachAllAndClear();
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

        private void TakePhoto()
        {
            _imagePicker.OpenCamera();
        }

        private void AddPhoto()
        {
            _imagePicker.OpenGallery();
        }

        private void RemoveAttachment()
        {
            _imagePicker.ViewModel.ImageCacheKey = null;
            _previewImageKey = null;
        }

        private void Send()
        {
            var args = _imagePicker.GetPickerData();
            _viewModelRef.Target.SendMessageCommand.Execute(new GenericEventArgs<ImagePickerArgs>(args));
            CloseEditImageContainer();
        }

        private void Initialize(Context context)
        {
            _bindings = new List<Binding>();
            _imagePicker = new ImagePicker(Dependencies.PermissionsManager, Dependencies.IocContainer.Resolve<IImagePickerService>());

            Inflate(context, Resource.Layout.view_chat_input, this);
            _messageEditText = FindViewById<EditText>(Resource.Id.et_conversations_message);

            _editingMessageCloseButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_editing_message_close);
            _editingMessageCloseButton.SetImageResource(StyleHelper.Style.EditingCloseButtonIcon);

            _takeAttachmentButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_take_attachment);
            _takeAttachmentButton.SetImageResource(StyleHelper.Style.TakeAttachmentButtonIcon);
            _takeAttachmentButton.SetCommand(new RelayCommand(TakePhoto));

            _addAttachmentButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_add_attachment);
            _addAttachmentButton.SetImageResource(StyleHelper.Style.AddAttachmentButtonIcon);
            _addAttachmentButton.SetCommand(new RelayCommand(AddPhoto));

            _sendButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_send);
            _sendButton.SetImageResource(StyleHelper.Style.SendMessageButtonIcon);
            _sendButton.SetCommand(nameof(_sendButton.Click), new RelayCommand(Send));

            _removeImageButton = FindViewById<ImageButton>(Resource.Id.activity_chat_conversations_remove_image_button);
            _removeImageButton.SetImageResource(StyleHelper.Style.RemoveImageButtonIcon);
            _removeImageButton.SetCommand(new RelayCommand(RemoveAttachment));

            _editImageContainer = FindViewById<View>(Resource.Id.activity_chat_conversations_image_preview_container);
            _editImageContainer.Visibility = ViewStates.Gone;

            _editingMessageHeader = FindViewById<TextView>(Resource.Id.tv_conversations_editing_message_header);

            _editingMessageBodyTextView = FindViewById<TextView>(Resource.Id.tv_editing_message_body);

            _editingMessageLayout = FindViewById<RelativeLayout>(Resource.Id.rl_conversations_editing_message);

            _imagePreview = FindViewById<ImageViewAsync>(Resource.Id.activity_chat_conversations_preview_image);
        }

        internal void HideKeyboard()
        {
            KeyboardService.HideSoftKeyboard(_messageEditText);
        }
    }
}
