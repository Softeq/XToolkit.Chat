// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Foundation;
using Softeq.XToolkit.Common.Weak;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public class ChatInputScrollViewDelegate : NSObject
    {
        private WeakReferenceEx<UIScrollView> _scrollView;
        private ChatInputView _chatInputView;
        private bool _needSkip;
        private nfloat _scrollViewBottomMargin;

        public ChatInputScrollViewDelegate(ChatInputView chatInputView, UIScrollView scrollView, nfloat scrollViewBottomMargin)
        {
            _scrollView = new WeakReferenceEx<UIScrollView>(scrollView);
            _chatInputView = chatInputView;
            _scrollViewBottomMargin = scrollViewBottomMargin;

            _chatInputView.KeyboardDelegate.KeyboardHeightChanged += KeyboardDelegate_KeyboardHeightChanged;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _chatInputView.KeyboardDelegate.KeyboardHeightChanged -= KeyboardDelegate_KeyboardHeightChanged;
            }
            base.Dispose(disposing);
        }

        private void KeyboardDelegate_KeyboardHeightChanged(nfloat height)
        {
            if (_scrollView.Target == null) { return; }

            if (_needSkip && _scrollView.Target?.ContentOffset.Y > 0)
            {
                _needSkip = false;
                return;
            }
            _needSkip = height == 0;

            var offset = (height == 0 ? _chatInputView.IntrinsicContentSize.Height : height) + _scrollViewBottomMargin;
            _scrollView.Target.ContentOffset = new CoreGraphics.CGPoint(0, -offset);
            _scrollView.Target.ContentInset = new UIEdgeInsets(offset, 0, 0, 0);
            _scrollView.Target.ScrollIndicatorInsets = new UIEdgeInsets(offset, 0, 0, 0);
        }
    }
}
