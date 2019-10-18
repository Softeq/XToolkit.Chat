// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using CoreGraphics;
using Foundation;
using Softeq.XToolkit.Common.Weak;
using Softeq.XToolkit.WhiteLabel.Threading;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Controls
{
    public interface IItemActionHandler<in T>
    {
        void Handle(UIView view, T viewModel);
    }

    public class ContextMenuHandler<T> : IItemActionHandler<T>, IDisposable
    {
        private readonly WeakFunc<T, ContextMenuComponent> _contextMenuComponentFunc;

        private ContextMenuComponent _lastContextMenuComponent;
        private T _viewModel;
        private IDisposable _notification;

        public ContextMenuHandler(Func<T, ContextMenuComponent> contextMenuComponentFunc)
        {
            _contextMenuComponentFunc = new WeakFunc<T, ContextMenuComponent>(contextMenuComponentFunc);
        }

        public void Handle(UIView view, T viewModel)
        {
            _viewModel = viewModel;
            _lastContextMenuComponent = _contextMenuComponentFunc.Execute(viewModel);

            if (_lastContextMenuComponent == null)
            {
                return;
            }

            var menuController = UIMenuController.SharedMenuController;
            if (menuController.MenuVisible)
            {
                return;
            }

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
            if (viewModel != null)
            {
                _lastContextMenuComponent?.ExecuteCommand(command, viewModel);
            }

            CleanUp();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanUp();
            }
        }

        protected virtual CGRect CalculateMenuPosition(UIView view)
        {
            var targetViewFrame = view.Frame;
            var targetRect = new CGRect(0, 0, targetViewFrame.Width, targetViewFrame.Height);
            return targetRect;
        }

        private void HideNotificationHandler(object sender, NSNotificationEventArgs e)
        {
            CleanUp(false);
        }

        private void CleanUp(bool cleanAll = true)
        {
            _notification?.Dispose();
            _notification = null;

            if (cleanAll)
            {
                _lastContextMenuComponent = null;
                _viewModel = default(T);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ContextMenuHandler()
        {
            Dispose(false);
        }
    }
}
