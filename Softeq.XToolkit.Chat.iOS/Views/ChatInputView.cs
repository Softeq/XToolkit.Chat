using CoreGraphics;
using Foundation;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;
using Softeq.XToolkit.WhiteLabel.iOS.ImagePicker;
using Softeq.XToolkit.Bindings;
using System;
using UIKit;
using FFImageLoading;
using Softeq.XToolkit.Common.EventArguments;
using Softeq.XToolkit.WhiteLabel.ImagePicker;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public sealed partial class ChatInputView : CustomViewBase
    {
        [Register("CustomTextView")]
        public class CustomTextView : UITextView
        {
            public CustomTextView(IntPtr handle) : base(handle) { }

            public override string Text 
            { 
                get => base.Text;
                set
                {
                    base.Text = value;
                    TextChanged?.Invoke();
                }
            }

            public event Action TextChanged;
        }

        private SimpleImagePicker _simpleImagePicker;
        private Binding _attachedImageBinding;

        public ChatInputView(IntPtr handle) : base(handle)
        {
        }

        public ChatInputView() : base(CGRect.Empty)
        {
        }

        public nfloat TopMargin => 8;

        public nfloat BottomMargin => Delegate.KeyBoardOpened ? 
            8 : 8 + UIApplication.SharedApplication.KeyWindow.SafeAreaInsets.Bottom;

        public ChatInputKeyboardDelegate Delegate { get; private set; }

        public override CGSize IntrinsicContentSize
        {
            get
            {
                var editContainerHeight = EditContainer.Hidden ? 
                    0 : EditContainer.SizeThatFits(new CGSize(EditContainer.Frame.Width, nfloat.MaxValue)).Height;
                var attachmentHeight = AttachmentContainer.Hidden ? 
                    0 : AttachmentContainer.SizeThatFits(new CGSize(AttachmentContainer.Frame.Width, nfloat.MaxValue)).Height;
                var textHeight = TextView.SizeThatFits(new CGSize(EditContainer.Frame.Width, nfloat.MaxValue)).Height;
                return new CGSize(Frame.Width, TopMargin + editContainerHeight + attachmentHeight + textHeight + BottomMargin);
            }
        }

        public event EventHandler<GenericEventArgs<ImagePickerArgs>> SendRaised;

        protected override void Initialize()
        {
            base.Initialize();
            Delegate = new ChatInputKeyboardDelegate(this);
            Delegate.KeyboardHeightChanged += Delegate_KeyboardFrameHeight;
            TextView.TextChanged += OnTextViewChanged;
            BottomConstraint.Constant = BottomMargin;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            Layer.BorderColor = UIColor.LightGray.CGColor;
            Layer.BorderWidth = 1;
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();
            if(Superview != null)
            {
                NSLayoutConstraint.ActivateConstraints(new []
                {
                    LeftAnchor.ConstraintEqualTo(Superview.LeftAnchor),
                    RightAnchor.ConstraintEqualTo(Superview.RightAnchor),
                    TopAnchor.ConstraintEqualTo(Superview.TopAnchor),
                });
                LayoutIfNeeded();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                TextView.TextChanged -= OnTextViewChanged;
                Delegate.KeyboardHeightChanged -= Delegate_KeyboardFrameHeight;
                Delegate.Dispose();
            }
            base.Dispose(disposing);
        }

        public void Init(UIViewController parentViewController)
        {
            _simpleImagePicker = new SimpleImagePicker(parentViewController, Dependencies.IocContainer.Resolve<IPermissionsManager>(), false);
            OpenCameraButton.SetCommand(new RelayCommand(_simpleImagePicker.OpenCameraAsync));
            OpenGalleryButton.SetCommand(new RelayCommand(_simpleImagePicker.OpenGalleryAsync));
            DeleteButton.SetCommand(new RelayCommand(OnDeleteButtonTap));
        }

        public void AttachBindings()
        {
            _attachedImageBinding = this.SetBinding(() => _simpleImagePicker.ViewModel.ImageCacheKey).WhenSourceChanges(() =>
            {
                InvokeOnMainThread(() =>
                {
                    if (string.IsNullOrEmpty(_simpleImagePicker.ViewModel.ImageCacheKey))
                    {
                        AttachmentImage.Image = null;
                        AttachmentContainer.Hidden = true;
                    }
                    else
                    {
                        AttachmentImage.Image = UIImage.FromFile(_simpleImagePicker.ViewModel.ImageCacheKey);
                        AttachmentContainer.Hidden = false;
                    }
                    LayoutIfNeeded();
                    InvalidateIntrinsicContentSize();
                });
            });
        }

        public void DetachBindings()
        {
            _attachedImageBinding.Detach();
        }

        public void StartEditing(string text)
        {
            EditingBodyLabel.Text = text;
            TextView.BecomeFirstResponder();
        }

        internal void ChangeEditingMode(bool isInEditMessageMode)
        {
            EditContainer.Hidden = !isInEditMessageMode;
            LayoutIfNeeded();
            InvalidateIntrinsicContentSize();
        }

        internal void SetLabels(string editMessageHeaderString, string enterMessagePlaceholderString)
        {
            EditingTitleLabel.Text = editMessageHeaderString;
            PlaceholderLabel.Text = enterMessagePlaceholderString;
        }

        internal void SetTextPlaceholdervisibility(bool isVisible)
        {
            PlaceholderLabel.Hidden = !isVisible;
        }

        private void Delegate_KeyboardFrameHeight(nfloat height)
        {
            BottomConstraint.Constant = BottomMargin;
            InvalidateIntrinsicContentSize();
        }

        private void OnDeleteButtonTap()
        {
            _simpleImagePicker.ViewModel.ImageCacheKey = null;
        }

        private void OnTextViewChanged()
        {
            var scrollEnabled = TextView.ContentSize.Height >= 300;
            if (scrollEnabled != TextView.ScrollEnabled)
            {
                TextMaxHeightConstraint.Active = scrollEnabled;
                TextView.ScrollEnabled = scrollEnabled;
            }
            LayoutIfNeeded();
            InvalidateIntrinsicContentSize();
        }

        partial void OnSend(UIButton sender)
        {
            var args = _simpleImagePicker.GetPickerData();
            SendRaised?.Invoke(this, new GenericEventArgs<ImagePickerArgs>(args));
            _simpleImagePicker.ViewModel.ImageCacheKey = null;
        }
    }
}