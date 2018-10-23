// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Linq;
using System.Collections.Generic;
using UIKit;
using ObjCRuntime;
using Softeq.XToolkit.WhiteLabel.Mvvm;

namespace Softeq.XToolkit.Chat.iOS.Controls
{
    public class ContextMenuComponent
    {
        private readonly IDictionary<string, CommandAction> _commandsMap = new Dictionary<string, CommandAction>();

        public void AddCommand(string key, CommandAction commandAction) => _commandsMap.Add(key, commandAction);

        public UIMenuItem[] BuildMenuItems()
        {
            return _commandsMap.Select(x => new UIMenuItem(x.Value.Title, new Selector(x.Key))).ToArray();
        }

        public void ExecuteCommand(string key, object parameter)
        {
            if (_commandsMap.TryGetValue(key, out CommandAction commandAction))
            {
                commandAction.Command.Execute(parameter);
            }
        }
    }
}
