// Developed by Softeq Development Corporation
// http://www.softeq.com
using System;
using CoreGraphics;
using Foundation;
using Softeq.XToolkit.WhiteLabel.Threading;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Controls
{
    public interface IItemActionHandler<in T>
    {
        void Handle(UIView view, T viewModel);
    }

    public class ContextMenuHandler<T> : IItemActionHandler<T>
    {
        private readonly Func<T, ContextMenuComponent> _contextMenuComponentFunc;
        private ContextMenuComponent _lastContextMenuComponent;
        private T _viewModel;
        private IDisposable _notification;

        public ContextMenuHandler(Func<T, ContextMenuComponent> contextMenuComponentFunc)
        {
            _contextMenuComponentFunc = contextMenuComponentFunc;
        }

        public void Handle(UIView view, T viewModel)
        {
            _lastContextMenuComponent = _contextMenuComponentFunc(viewModel);
            _viewModel = viewModel;

            var menuController = UIMenuController.SharedMenuController;

            _notification?.Dispose();
            _notification = UIMenuController.Notifications.ObserveWillHideMenu(menuController, HideNotificationHandler);

            Execute.BeginOnUIThread(() =>
            {
                if (!menuController.MenuVisible)
                {
                    var targetRect = CalculateMenuPosition(view);

                    menuController.MenuItems = _lastContextMenuComponent.BuildMenuItems();
                    menuController.Update();
                    menuController.SetTargetRect(targetRect, view);
                    menuController.SetMenuVisible(true, true);
                }
            });
        }

        public void ExecuteCommand(string command)
        {
            object viewModel = _viewModel;
            if (viewModel != null && _lastContextMenuComponent != null)
            {
                _lastContextMenuComponent.ExecuteCommand(command, viewModel);
            }
            _notification?.Dispose();
        }

        protected virtual CGRect CalculateMenuPosition(UIView view)
        {
            var targetViewFrame = view.Frame;
            var targetRect = new CGRect(0, 0, targetViewFrame.Width, targetViewFrame.Height);
            return targetRect;
        }

        private void HideNotificationHandler(object sender, NSNotificationEventArgs e)
        {
            _lastContextMenuComponent = null;
            _viewModel = default(T);
        }
    }
}
