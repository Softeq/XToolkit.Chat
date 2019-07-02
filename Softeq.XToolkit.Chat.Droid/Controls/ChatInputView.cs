// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using FFImageLoading.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.WhiteLabel.Droid.Services;
using Softeq.XToolkit.Bindings.Extensions;

namespace Softeq.XToolkit.Chat.Droid.Controls
{
    [Register("com.softeq.xtoolkit.chat.droid.ChatInputView")]
    public class ChatInputView : LinearLayout
    {
        private WeakReferenceEx<ChatMessageInputViewModel> _viewModelRef;
        private List<Binding> _bindings;
        private bool _inited;

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
            _bindings.Add(this.SetBinding(() => _viewModelRef.Target.ImageObject).WhenSourceChanges(() =>
            {
                if (_viewModelRef.Target.ImageObject == null)
                {
                    _imagePreview.SetImageBitmap(null);
                    _editImageContainer.Visibility = ViewStates.Gone;
                }
                else
                {
                    _imagePreview.SetImageBitmap((Android.Graphics.Bitmap) _viewModelRef.Target.ImageObject);
                    _editImageContainer.Visibility = ViewStates.Visible;
                }
            }));

            if (!_inited)
            {
                _messageEditText.Hint = _viewModelRef.Target.EnterMessagePlaceholderString;
                _editingMessageHeader.Text = _viewModelRef.Target.EditMessageHeaderString;

                _takeAttachmentButton.SetCommand(_viewModelRef.Target.OpenCameraCommand);
                _addAttachmentButton.SetCommand(_viewModelRef.Target.OpenGalleryCommand);
                _sendButton.SetCommand(_viewModelRef.Target.SendMessageCommand);
                _removeImageButton.SetCommand(_viewModelRef.Target.DeleteImageCommand);
                _editingMessageCloseButton.SetCommand(_viewModelRef.Target.CancelEditingCommand);
                _inited = true;
            }
        }

        public void UnbindViewModel()
        {
            _bindings.DetachAllAndClear();
        }

        private void Initialize(Context context)
        {
            _bindings = new List<Binding>();

            Inflate(context, Resource.Layout.view_chat_input, this);
            _messageEditText = FindViewById<EditText>(Resource.Id.et_conversations_message);

            _editingMessageCloseButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_editing_message_close);
            _editingMessageCloseButton.SetImageResource(StyleHelper.Style.EditingCloseButtonIcon);

            _takeAttachmentButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_take_attachment);
            _takeAttachmentButton.SetImageResource(StyleHelper.Style.TakeAttachmentButtonIcon);

            _addAttachmentButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_add_attachment);
            _addAttachmentButton.SetImageResource(StyleHelper.Style.AddAttachmentButtonIcon);

            _sendButton = FindViewById<ImageButton>(Resource.Id.ib_conversations_send);
            _sendButton.SetImageResource(StyleHelper.Style.SendMessageButtonIcon);

            _removeImageButton = FindViewById<ImageButton>(Resource.Id.activity_chat_conversations_remove_image_button);
            _removeImageButton.SetImageResource(StyleHelper.Style.RemoveImageButtonIcon);

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
