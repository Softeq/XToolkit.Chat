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

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public sealed partial class ChatInputView : CustomViewBase
    {
        private SimpleImagePicker _simpleImagePicker;
        private Binding _attachedImageBinding;

        public ChatInputView(IntPtr handle) : base(handle)
        {
        }

        public ChatInputView() : base(CGRect.Empty)
        {
        }

        public event EventHandler<GenericEventArgs<ImagePickerArgs>> SendRaised;

        protected override void Initialize()
        {
            base.Initialize();
            TextView.Changed += OnTextViewChanged;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            Layer.BorderColor = UIColor.LightGray.CGColor;
            Layer.BorderWidth = 1;
        }

        public void Init(UIViewController parentViewController, ChatInputKeyboardDelegate chatInputKeyboardDelegate)
        {
            _simpleImagePicker = new SimpleImagePicker(parentViewController, Dependencies.IocContainer.Resolve<IPermissionsManager>(), false);
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
                });
            });
            OpenCameraButton.SetCommand(new RelayCommand(_simpleImagePicker.OpenCameraAsync));
            OpenGalleryButton.SetCommand(new RelayCommand(_simpleImagePicker.OpenGalleryAsync));
            DeleteButton.SetCommand(new RelayCommand(OnDeleteButtonTap));
        }

        public void StartEditing(string text)
        {
            EditingBodyLabel.Text = text;
        }

        private void OnDeleteButtonTap()
        {
            _simpleImagePicker.ViewModel.ImageCacheKey = null;
        }

        private void OnTextViewChanged(object sender, EventArgs e)
        {
            var scrollEnabled = TextView.ContentSize.Height >= 300;
            if (scrollEnabled != TextView.ScrollEnabled)
            {
                TextMaxHeightConstraint.Active = scrollEnabled;
                TextView.ScrollEnabled = scrollEnabled;
                TextView.InvalidateIntrinsicContentSize();
            }
        }

        partial void OnSend(UIButton sender)
        {
            var args = _simpleImagePicker.GetPickerData();
            SendRaised?.Invoke(this, new GenericEventArgs<ImagePickerArgs>(args));
            _simpleImagePicker.ViewModel.ImageCacheKey = null;
        }

        internal void ChangeEditingMode(bool isInEditMessageMode)
        {
            EditContainer.Hidden = !isInEditMessageMode;
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
    }
}