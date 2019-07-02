// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using System;
using Foundation;
using UIKit;
using Softeq.XToolkit.Common;

namespace Softeq.XToolkit.Chat.iOS.TableSources
{
    public class ActionableTableViewDelegate : UITableViewDelegate
    {
        private readonly WeakFunc<UITableViewRowAction[]> _getActionsForRow;

        public ActionableTableViewDelegate(Func<UITableViewRowAction[]> getActionsForRow)
        {
            _getActionsForRow = new WeakFunc<UITableViewRowAction[]>(getActionsForRow);
        }

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
        {
            if (_getActionsForRow != null && _getActionsForRow.IsAlive)
            {
                return _getActionsForRow.Execute();
            }
            return new UITableViewRowAction[] { };
        }
    }
}
