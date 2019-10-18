// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Foundation;
using Softeq.XToolkit.Common.Weak;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public class ChatInputKeyboardDelegate : NSObject
    {
        private readonly IDisposable _willShowObserver;
        private readonly IDisposable _willHideObserver;
        private readonly IDisposable _willChangeFrameObserver;

        private WeakReferenceEx<ChatInputView> _chatInputView;

        public ChatInputKeyboardDelegate(ChatInputView chatInputView)
        {
            _chatInputView = new WeakReferenceEx<ChatInputView>(chatInputView);
            _willShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, (notification) =>
            {
                if (!KeyBoardOpened)
                {
                    TryInvokeOpenKeyboardEvent(notification);
                }
            });
            _willHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, (notification) =>
            {
                if (KeyBoardOpened)
                {
                    KeyBoardOpened = false;
                    KeyboardHeightChanged?.Invoke(0);
                }
            });
            _willChangeFrameObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillChangeFrameNotification, (notification) =>
            {
                if (KeyBoardOpened)
                {
                    TryInvokeOpenKeyboardEvent(notification);
                }
            });
        }

        public bool KeyBoardOpened { get; private set; }

        public event Action<nfloat> KeyboardHeightChanged;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _willShowObserver.Dispose();
                _willHideObserver.Dispose();
                _willChangeFrameObserver?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void TryInvokeOpenKeyboardEvent(NSNotification notification)
        {
            var keyboardHeight = UIKeyboard.FrameEndFromNotification(notification).Height;
            if (_chatInputView.Target?.Frame.Height < keyboardHeight)
            {
                KeyBoardOpened = true;
            }
            KeyboardHeightChanged?.Invoke(keyboardHeight);
        }
    }
}
