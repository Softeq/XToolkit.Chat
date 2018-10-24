// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using CoreGraphics;
using UIKit;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ChatMessagesInputBarView : CustomViewBase
    {
        private nfloat _editViewContainerInitialHeight;
        private bool _shouldSkipUpdateInputHeight;
        private CGSize _cachedIntrinsicContentSize;

        public ChatMessagesInputBarView(IntPtr handle) : base(handle) { }
        public ChatMessagesInputBarView(CGRect frame) : base(frame) { }

        public event EventHandler<nfloat> TopContainersHeightChanged;

        public override CGSize IntrinsicContentSize => _cachedIntrinsicContentSize;

        public nfloat ContainerHeight => CalculateContainerHeight();
        public UITextView Input => InputTextView;
        public UIButton AttachImage => AttachImageButton;
        public UIButton Send => SendButton;
        public UIButton EditingClose => EditingCloseButton;

        private double SafeAreaBottomInset => UIDevice.CurrentDevice.CheckSystemVersion(11, 0) ? SafeAreaInsets.Bottom : 0;

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

        public void SetInputPlaceholderHidden(bool isHidden)
        {
            InputTextViewPlaceholder.Hidden = isHidden;
        }

        public void StartEditing(string originalMessageBody)
        {
            EditingText.Text = originalMessageBody;
            InputTextView.BecomeFirstResponder();
        }

        public void ChangeEditingMode(bool isInEditMessageMode)
        {
            var isEditViewContainerHidden = !isInEditMessageMode;

            InvokeTopContainersHeightChangedIfNeeded(isEditViewContainerHidden);

            CollapseEditViewContainer(isEditViewContainerHidden);
            InputViewTopBorder.Hidden = isInEditMessageMode;
            SetInputPlaceholderHidden(isInEditMessageMode);

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

        protected override void Initialize()
        {
            base.Initialize();

            AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            EditingIndicatorView.Layer.CornerRadius = 2f;

            // TODO: setup attachments view
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
            // TODO: add attachments height
            return InputViewContainer.Frame.Height + EditViewContainerHeightConstraint.Constant;
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

        private void InvokeTopContainersHeightChangedIfNeeded(bool isEditViewContainerHidden)
        {
            if (EditViewContainer.Hidden == isEditViewContainerHidden)
            {
                return;
            }

            var changedHeight = EditViewContainerHeightConstraint.Constant;
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
    }
}