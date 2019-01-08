using System;
using Foundation;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.TableSources
{
    public class ActionableTableViewDelegate : UITableViewDelegate
    {
        private readonly Func<UITableViewRowAction[]> _getActionsForRow;

        public ActionableTableViewDelegate(Func<UITableViewRowAction[]> getActionsForRow)
        {
            _getActionsForRow = getActionsForRow;
        }

        public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return _getActionsForRow();
        }
    }
}
