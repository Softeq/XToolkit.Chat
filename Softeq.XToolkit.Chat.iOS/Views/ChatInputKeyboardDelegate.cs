using System;
using Foundation;
using Softeq.XToolkit.Common;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public class ChatInputKeyboardDelegate : IDisposable
    {
        private readonly IDisposable _willShowObserver;
        private readonly IDisposable _willHideObserver;

        public ChatInputKeyboardDelegate(NSLayoutConstraint inputBottomConstraint)
        {
            _willShowObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, (notification) =>
            {
                var safeAreaBottom = UIApplication.SharedApplication.KeyWindow.SafeAreaInsets.Bottom;
                var keyBoardHeight = UIKeyboard.FrameEndFromNotification(notification).Height;
                inputBottomConstraint.Constant = keyBoardHeight - safeAreaBottom;
            });
            _willHideObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillHideNotification, (notification) =>
            {
                inputBottomConstraint.Constant = 0;
            });
        }

        public void Dispose()
        {
            _willShowObserver.Dispose();
            _willHideObserver.Dispose();
        }
    }
}
