﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using CoreGraphics;
using FFImageLoading;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Commands;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.ImagePicker;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;
using Softeq.XToolkit.WhiteLabel.iOS.ImagePicker;
using Softeq.XToolkit.WhiteLabel.Threading;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    //TODO VPY: we need to review this class
    [Obsolete("Use ChatInputView class")]
    public partial class ChatMessagesInputBarView : CustomViewBase
    {
        private Binding _attachedImageBinding;
        private nfloat _cachedHeight;
        private CGSize _cachedIntrinsicContentSize;
        private nfloat _editViewContainerInitialHeight;
        private string _previewImageKey;
        private bool _shouldSkipUpdateInputHeight;
        private SimpleImagePicker _simpleImagePicker;

        public ChatMessagesInputBarView(IntPtr handle) : base(handle)
        {
        }

        public ChatMessagesInputBarView(CGRect frame) : base(frame)
        {
        }

        public override CGSize IntrinsicContentSize => _cachedIntrinsicContentSize;

        public nfloat ContainerHeight => CalculateContainerHeight();
        public UITextView Input => InputTextView;
        public UIButton EditingClose => EditingCloseButton;

        private double SafeAreaBottomInset =>
            UIDevice.CurrentDevice.CheckSystemVersion(11, 0) ? SafeAreaInsets.Bottom : 0;

        public event EventHandler<nfloat> TopContainersHeightChanged;
        public event EventHandler<GenericEventArgs<ImagePickerArgs>> SendRaised;
        public event EventHandler PickerWillOpen;

        public void UpdateInputHeight(CGSize newSize)
        {
            // prevent duplication call after change contentSize below
            if (_shouldSkipUpdateInputHeight)
            {
                _shouldSkipUpdateInputHeight = false;
                return;
            }

            _shouldSkipUpdateInputHeight = true;

            InputTextView.ScrollEnabled = newSize.Height >= InputTextViewMaxHeightConstraint.Constant;

            // contentSize is not adjusted correctly when scrollEnabled is changed
            InputTextView.ContentSize = InputTextView.SizeThatFits(InputTextView.Frame.Size);

            InputTextView.InvalidateIntrinsicContentSize();
            InvalidateIntrinsicContentSize();
        }

        public void StartEditing(string originalMessageBody)
        {
            EditingText.Text = originalMessageBody;
            InputTextView.BecomeFirstResponder();
        }

        public void ChangeEditingMode(bool isInEditMessageMode)
        {
            var isEditViewContainerHidden = !isInEditMessageMode;

            CollapseEditViewContainer(isEditViewContainerHidden);

            InvokeTopContainersHeightChangedIfNeeded();
            InvalidateIntrinsicContentSize();
        }

        public void KeyboardChanged()
        {
            InvalidateIntrinsicContentSize();
        }

        public override void InvalidateIntrinsicContentSize()
        {
            base.InvalidateIntrinsicContentSize();

            _cachedIntrinsicContentSize = CalculateIntrinsicContentSize();
        }

        public void ViewDidLoad(UIViewController viewController)
        {
            SendButton.TintColor = StyleHelper.Style.ButtonTintColor;
            SendButton.SetImage(UIImage.FromBundle(StyleHelper.Style.SendBundleName), UIControlState.Normal);
            SendButton.SetCommand(new RelayCommand(OnRaiseSend));

            AttachImageButton.TintColor = StyleHelper.Style.ButtonTintColor;
            AttachImageButton.SetImage(UIImage.FromBundle(StyleHelper.Style.AddImageBundleName), UIControlState.Normal);
            AttachImageButton.SetCommand(new RelayCommand(OnAddPhotoClicked));

            TakePhotoButton.TintColor = StyleHelper.Style.ButtonTintColor;
            TakePhotoButton.SetImage(UIImage.FromBundle(StyleHelper.Style.TakePhotoBundleName), UIControlState.Normal);
            TakePhotoButton.SetCommand(new RelayCommand(OnTakePhotoClicked));

            RemoveAttachButton.SetImage(UIImage.FromBundle(StyleHelper.Style.RemoveAttachBundleName),
                UIControlState.Normal);
            RemoveAttachButton.SetCommand(new RelayCommand(OnRemovePhotoClicked));

            AttachedImageView.Layer.MasksToBounds = false;
            AttachedImageView.Layer.CornerRadius = 5;
            AttachedImageView.ClipsToBounds = true;
            AttachedImageView.ContentMode = UIViewContentMode.ScaleAspectFill;

            EditImageContainer.Hidden = true;
            EditImageContainerHeightConstraint.Constant = 0;

            _simpleImagePicker = new SimpleImagePicker(viewController,
                Dependencies.Container.Resolve<IPermissionsManager>(), false);
            _attachedImageBinding = this.SetBinding(() => _simpleImagePicker.ViewModel.ImageCacheKey).WhenSourceChanges(
                () =>
                {
                    if (string.IsNullOrEmpty(_simpleImagePicker.ViewModel.ImageCacheKey))
                    {
                        CloseAttachPanel();
                        return;
                    }

                    OpenAttachPanel();
                });
            _simpleImagePicker.SetCommand(nameof(_simpleImagePicker.PickerWillOpen),
                new RelayCommand(RaisePickerWillOpen));
        }

        public void SetTextPlaceholdervisibility(bool isVisible)
        {
            InputTextViewPlaceholder.Hidden = !isVisible;
        }

        public void SetLabels(string editMessageLabel, string inputPlaceholder)
        {
            EditMessageHeaderLabel.Text = editMessageLabel;
            InputTextViewPlaceholder.Text = inputPlaceholder;
        }

        protected override void Initialize()
        {
            base.Initialize();

            AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            EditingIndicatorView.Layer.CornerRadius = 2f;
            EditingIndicatorView.BackgroundColor = StyleHelper.Style.AccentColor;

            EditMessageHeaderLabel.TextColor = StyleHelper.Style.AccentColor;

            _editViewContainerInitialHeight = EditViewContainerHeightConstraint.Constant;
            UpdateEditViewHeightConstraint();

            _cachedIntrinsicContentSize = CalculateIntrinsicContentSize();
        }

        private CGSize CalculateIntrinsicContentSize()
        {
            var requiredHeight = CalculateContainerHeight() + SafeAreaBottomInset;
            return new CGSize(Bounds.Width, requiredHeight);
        }

        private nfloat CalculateContainerHeight()
        {
            return InputViewContainer.Frame.Height
                   + EditViewContainerHeightConstraint.Constant
                   + EditImageContainerHeightConstraint.Constant;
        }

        private void CollapseEditViewContainer(bool isHidden)
        {
            EditViewContainer.Hidden = isHidden;
            UpdateEditViewHeightConstraint();
        }

        private void UpdateEditViewHeightConstraint()
        {
            EditViewContainerHeightConstraint.Constant = EditViewContainer.Hidden ? 0 : _editViewContainerInitialHeight;
        }

        private void InvokeTopContainersHeightChangedIfNeeded()
        {
            var changedHeight = EditViewContainerHeightConstraint.Constant
                                + EditImageContainerHeightConstraint.Constant;

            if (changedHeight == _cachedHeight)
            {
                return;
            }

            _cachedHeight = changedHeight;

            if (changedHeight > 0)
            {
                changedHeight *= -1;
            }
            else
            {
                changedHeight = _editViewContainerInitialHeight;
            }

            TopContainersHeightChanged?.Invoke(this, changedHeight);
        }

        private void OnAddPhotoClicked()
        {
            _simpleImagePicker.OpenGalleryAsync();
        }

        private void OnTakePhotoClicked()
        {
            _simpleImagePicker.OpenCameraAsync();
        }

        private void OnRemovePhotoClicked()
        {
            _simpleImagePicker.ViewModel.ImageCacheKey = null;
        }

        private void OpenAttachPanel()
        {
            Execute.BeginOnUIThread(() =>
            {
                var key = _simpleImagePicker.ViewModel.ImageCacheKey;
                if (key == _previewImageKey)
                {
                    return;
                }

                EditImageContainerHeightConstraint.Constant = 72;
                EditImageContainer.Hidden = false;
                InputTextView.BecomeFirstResponder();
                InvokeTopContainersHeightChangedIfNeeded();
                InvalidateIntrinsicContentSize();

                _previewImageKey = key;

                ImageService.Instance
                    .LoadFile(key)
                    .DownSampleInDip(60, 60)
                    .IntoAsync(AttachedImageView);
            });
        }

        private void CloseAttachPanel()
        {
            Execute.BeginOnUIThread(() =>
            {
                _previewImageKey = null;
                _simpleImagePicker.ViewModel.ImageCacheKey = null;
                AttachedImageView.Image?.Dispose();
                AttachedImageView.Image = null;
                EditImageContainer.Hidden = true;
                EditImageContainerHeightConstraint.Constant = 0;
                InvokeTopContainersHeightChangedIfNeeded();
                InvalidateIntrinsicContentSize();
            });
        }

        private void OnRaiseSend()
        {
            var args = _simpleImagePicker.GetPickerData();
            SendRaised?.Invoke(this, new GenericEventArgs<ImagePickerArgs>(args));
            CloseAttachPanel();
        }

        private void RaisePickerWillOpen()
        {
            PickerWillOpen?.Invoke(this, EventArgs.Empty);
        }
    }
}