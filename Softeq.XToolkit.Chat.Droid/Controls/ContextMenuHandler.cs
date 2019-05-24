// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.Support.V7.Widget;
using Android.Views;

namespace Softeq.XToolkit.Chat.Droid.Controls
{
    public interface IItemActionHandler<in T>
    {
        void Handle(View itemView, T viewModel);
    }

    public class ContextMenuHandler<T> : IItemActionHandler<T>
    {
        private readonly Func<T, ContextMenuComponent> _contextMenuFunc;

        private T _viewModel;
        private ContextMenuComponent _lastContextMenuComponent;

        public ContextMenuHandler(Func<T, ContextMenuComponent> contextMenuFunc)
        {
            _contextMenuFunc = contextMenuFunc;
        }

        public void Handle(View itemView, T viewModel)
        {
            _viewModel = viewModel;
            _lastContextMenuComponent = _contextMenuFunc(viewModel);

            var popup = _lastContextMenuComponent.BuildMenu(itemView.Context, itemView);
            popup.MenuItemClick += PopupMenuItemClickHandler;
            popup.DismissEvent += PopupDismissEvent;
            popup.Show();
        }

        private void PopupDismissEvent(object sender, PopupMenu.DismissEventArgs e)
        {
            _lastContextMenuComponent = null;
            _viewModel = default(T);
        }

        private void PopupMenuItemClickHandler(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            object viewModel = _viewModel;
            if (viewModel == null || _lastContextMenuComponent == null)
            {
                return;
            }

            _lastContextMenuComponent.ExecuteCommand(e.Item.ItemId, viewModel);
        }
    }
}
