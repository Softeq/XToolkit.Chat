// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using PopupMenu = Android.Support.V7.Widget.PopupMenu;

namespace Softeq.XToolkit.Chat.Droid.Controls
{
    public class ContextMenuComponent
    {
        private readonly IDictionary<int, CommandAction> _commandActions = new Dictionary<int, CommandAction>();

        public ContextMenuComponent(IReadOnlyList<CommandAction> commandActions)
        {
            commandActions.Apply(commandAction => _commandActions.Add(View.GenerateViewId(), commandAction));
        }

        public PopupMenu BuildMenu(Context context, View anchorView)
        {
            var popup = new PopupMenu(context, anchorView);
            var order = 0;
            foreach (var commandAction in _commandActions)
            {
                popup.Menu.Add(0, commandAction.Key, order++, commandAction.Value.Title);
            }
            return popup;
        }

        public void ExecuteCommand(int menuItemId, object parameter)
        {
            if (_commandActions.TryGetValue(menuItemId, out CommandAction commandAction))
            {
                commandAction.Command.Execute(parameter);
            }
        }
    }
}
