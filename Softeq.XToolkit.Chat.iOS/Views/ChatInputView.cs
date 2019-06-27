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
using Softeq.XToolkit.Common;
using System.Threading.Tasks;
using System.IO;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public sealed partial class ChatInputView : CustomViewBase
    {
        [Register("CustomTextView")]
        public class CustomTextView : UITextView
        {
            public CustomTextView(IntPtr handle) : base(handle) 
            {
                Initialize();
            }

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

            private void Initialize ()
            {
                Changed += (sender, e) => TextChanged?.Invoke();
            }
        }

        public ChatInputView(IntPtr handle) : base(handle)
        {
        }

        public ChatInputView() : base(CGRect.Empty)
        {
        }

        public nfloat TopMargin => 8;

        public nfloat BottomMargin => KeyboardDelegate.KeyBoardOpened ? 
            8 : 8 + UIApplication.SharedApplication.KeyWindow.SafeAreaInsets.Bottom;

        public ChatInputKeyboardDelegate KeyboardDelegate { get; private set; }

        public ChatInputScrollViewDelegate ScrollViewDelegate { get; private set; }

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

        protected override void Initialize()
        {
            base.Initialize();
            KeyboardDelegate = new ChatInputKeyboardDelegate(this);
            KeyboardDelegate.KeyboardHeightChanged += Delegate_KeyboardFrameHeight;
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
                KeyboardDelegate.KeyboardHeightChanged -= Delegate_KeyboardFrameHeight;
                KeyboardDelegate.Dispose();
            }
            base.Dispose(disposing);
        }

        public void Init(UIViewController parentViewController)
        {
        }

        public void SetBoundedScrollView(UIScrollView boundedScrollView, nfloat scrollViewBottomMargin)
        {
            ScrollViewDelegate = new ChatInputScrollViewDelegate(this, boundedScrollView, scrollViewBottomMargin);
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

        internal void SetImage(object imageObject)
        {
            if (imageObject == null)
            {
                AttachmentImage.Image = null;
                AttachmentContainer.Hidden = true;
            }
            else
            {
                AttachmentImage.Image = (UIImage) imageObject;
                AttachmentContainer.Hidden = false;
            }
            LayoutIfNeeded();
            InvalidateIntrinsicContentSize();
        }
    }
}