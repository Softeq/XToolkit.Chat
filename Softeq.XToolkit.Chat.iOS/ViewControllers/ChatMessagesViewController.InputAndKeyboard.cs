// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using CoreGraphics;
using UIKit;
using Foundation;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Common.WeakSubscription;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class ChatMessagesViewController
    {
        private const int RoundDigits = 5;

        private IDisposable _keyboardWillShowObserver;
        private IDisposable _keyboardWillHideObserver;
        private IDisposable _textViewEditingStartedObserver;
        private IDisposable _textViewEditingEndedObserver;
        private IDisposable _textViewContentSizeObserver;
        private IDisposable _topContainersHeightChangedSubscription;
        private nfloat _cachedScrollDownBottomConstraintValue;
        private bool _isKeyboardOpened;
        private double _lastInputBarChangedOffset;

        public override UIView InputAccessoryView => InputBar;

        public override bool CanBecomeFirstResponder => true;

        private nfloat SafeAreaBottomOffset
        {
            get
            {
                var window = UIApplication.SharedApplication.KeyWindow;
                return UIDevice.CurrentDevice.CheckSystemVersion(11, 0) && window != null ? window.SafeAreaInsets.Bottom : 0;
            }
        }
        private ChatMessagesInputBarView InputBar { get; set; }
        private nfloat InputBarHeight => InputBar.ContainerHeight + SafeAreaBottomOffset;

        private void SetupInputAccessoryView()
        {
            InputBar = new ChatMessagesInputBarView(CGRect.Empty);

            _cachedScrollDownBottomConstraintValue = DefaultScrollDownBottomConstraintValue + InputBarHeight;
            ScrollDownBottomConstraint.Constant = _cachedScrollDownBottomConstraintValue;
        }

        private void RegisterKeyboardObservers()
        {
            _keyboardWillShowObserver = UIKeyboard.Notifications.ObserveWillShow((sender, e) =>
            {
                var keyboardHeight = e.FrameEnd.Height;

                // skip handling when the keyboard is hidden
                if (!(Math.Round(keyboardHeight, RoundDigits) > Math.Round(InputBarHeight, RoundDigits)))
                {
                    return;
                }

                UpdateContentOffsetsForOpenedKeyboard(keyboardHeight);

                _isKeyboardOpened = true;
            });

            _keyboardWillHideObserver = UIKeyboard.Notifications.ObserveWillHide((sender, e) =>
            {
                // on iPhoneX will called 3 times (collapse safeAreaBottom)
                if (_isKeyboardOpened)
                {
                    UpdateContentOffsetsForClosedKeyboard(e.FrameEnd.Height + SafeAreaBottomOffset);
                }

                _isKeyboardOpened = false;
            });

            _textViewEditingStartedObserver = UITextView.Notifications.ObserveTextDidBeginEditing((sender, e) =>
            {
                if (!e.Notification.Object.Handle.Equals(InputBar.Input.Handle))
                {
                    return;
                }
                InputBar.SetInputPlaceholderHidden(true);
            });

            _textViewEditingEndedObserver = UITextView.Notifications.ObserveTextDidEndEditing((sender, e) =>
            {
                if (!e.Notification.Object.Handle.Equals(InputBar.Input.Handle))
                {
                    return;
                }
                InputBar.SetInputPlaceholderHidden(!string.IsNullOrEmpty(ViewModel.MessageToSendBody));
            });

            _textViewContentSizeObserver = InputBar.Input.AddObserver(ContentSizeKey, NSKeyValueObservingOptions.OldNew, e =>
            {
                var newSize = ((NSValue)e.NewValue).CGSizeValue;

                InputBar.UpdateInputHeight(newSize);
            });

            if (InputBar != null)
            {
                _topContainersHeightChangedSubscription = new WeakEventSubscription<ChatMessagesInputBarView, nfloat>(
                    InputBar, nameof(InputBar.TopContainersHeightChanged),
                    (sender, changedHeight) => InputBarTopContainersHeightChanged(changedHeight));
            }
        }

        private void UnregisterKeyboardObservers()
        {
            _keyboardWillShowObserver?.Dispose();
            _keyboardWillShowObserver = null;

            _keyboardWillHideObserver?.Dispose();
            _keyboardWillHideObserver = null;

            _textViewEditingStartedObserver?.Dispose();
            _textViewEditingStartedObserver = null;

            _textViewEditingEndedObserver?.Dispose();
            _textViewEditingEndedObserver = null;

            _textViewContentSizeObserver?.Dispose();
            _textViewContentSizeObserver = null;

            _topContainersHeightChangedSubscription?.Dispose();
            _topContainersHeightChangedSubscription = null;
        }

        private void HideKeyboard()
        {
            InputBar.Input.ResignFirstResponder();
        }

        private void UpdateContentOffsetsForOpenedKeyboard(nfloat keyboardHeight)
        {
            UpdateTableViewTopInset(keyboardHeight + _tableViewBottomConstraint.Constant);

            double newBottomOffset;

            if (_isKeyboardOpened)
            {
                newBottomOffset = CalculateOffsetForMultilineInput(keyboardHeight);
            }
            else
            {
                newBottomOffset = CalculateOffsetForShowKeyboard(keyboardHeight);
            }

            UpdateTableViewContentOffsetY(newBottomOffset);

            InputBar.KeyboardChanged();

            UpdateScrollDownButtonPositionWithAnimation();
        }

        private void UpdateContentOffsetsForClosedKeyboard(nfloat keyboardHeight)
        {
            var inputMultilineHeight = (nfloat)Math.Round(InputBarHeight - Math.Abs(_tableViewBottomConstraint.Constant), RoundDigits);
            UpdateTableViewTopInset(inputMultilineHeight);

            var newBottomOffset = CalculateOffsetForHideKeyboard(keyboardHeight, inputMultilineHeight);
            UpdateTableViewContentOffsetY(newBottomOffset);

            InputBar.KeyboardChanged();

            UpdateScrollDownButtonPositionWithAnimation();

            _lastInputBarChangedOffset = inputMultilineHeight;
        }

        private double CalculateOffsetForMultilineInput(nfloat keyboardHeight)
        {
            var inputBarChangedOffset = Math.Round(InputBarHeight - Math.Abs(_tableViewBottomConstraint.Constant), RoundDigits);
            var visibleTableViewBottomOffset = Math.Round(TableNode.View.ContentOffset.Y + keyboardHeight - InputBarHeight, RoundDigits);
            var linesHeight = _lastInputBarChangedOffset - inputBarChangedOffset;

            if (_lastInputBarChangedOffset > inputBarChangedOffset && visibleTableViewBottomOffset <= 0)
            {
                linesHeight = 0; // when lines removed
            }
            _lastInputBarChangedOffset = inputBarChangedOffset;

            return TableNode.View.ContentOffset.Y + linesHeight;
        }

        private double CalculateOffsetForShowKeyboard(nfloat keyboardHeight)
        {
            var keyboardCutOutHeight = InputBarHeight + SafeAreaBottomOffset;
            return TableNode.View.ContentOffset.Y - (keyboardHeight - keyboardCutOutHeight);
        }

        private double CalculateOffsetForHideKeyboard(nfloat keyboardHeight, nfloat footerOffset)
        {
            if (TableNode.View.ContentOffset.Y <= 0)
            {
                return -footerOffset;
            }

            return TableNode.View.ContentOffset.Y + keyboardHeight - InputBarHeight - SafeAreaBottomOffset;
        }

        private void UpdateTableViewTopInset(nfloat topInset)
        {
            var newInsets = new UIEdgeInsets(topInset, 0, 0, 0);
            TableNode.View.ContentInset = newInsets;
            TableNode.View.ScrollIndicatorInsets = newInsets;
        }

        private void UpdateTableViewContentOffsetY(double newOffsetY)
        {
            TableNode.View.SetContentOffset(new CGPoint(0, newOffsetY), false);
        }

        private void InputBarTopContainersHeightChanged(nfloat changedHeight)
        {
            UpdateTableViewTopInset(TableNode.View.ContentInset.Top + changedHeight);

            if (!_isKeyboardOpened)
            {
                var newBottomOffset = TableNode.View.ContentOffset.Y - changedHeight;

                if (TableNode.View.ContentOffset.Y == 0)
                {
                    newBottomOffset = 0;
                }
                //TODO: edit -> add multilines -> close keyboard -> editing close (offset on footer == multiline height)

                UpdateTableViewContentOffsetY(newBottomOffset);
            }
            //TODO: when keyboard open InputBarHeight sometimes without EditingHeight (because got before bind IsInEditMessageMode)
            //       reproduce: edit -> close keyboard -> editing close -> start editing (need remove offset to bottom)
            //       check CalculateOffsetForShowKeyboard
            //else
            //{
            //    var a = TableNode.View.ContentOffset.Y - changedHeight;
            //    UpdateTableViewContentOffsetY(a);
            //}

            if (changedHeight < 0)
            {
                ScrollDownBottomConstraint.Constant = ScrollDownBottomConstraint.Constant + changedHeight;
            }
        }
    }
}
