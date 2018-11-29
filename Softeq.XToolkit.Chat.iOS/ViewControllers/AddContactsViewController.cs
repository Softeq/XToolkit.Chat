// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Bindings;
using UIKit;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class AddContactsViewController : ViewControllerBase<AddContactsViewModel>
    {
        public AddContactsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CustomNavigationBarItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.CloseButtonImageBoundleName),
                ViewModel.CancelCommand,
                true);
            CustomNavigationBarItem.SetCommand(
                "Done",
                StyleHelper.Style.NavigationBarTintColor,
                ViewModel.DoneCommand,
                false);

            TableView.AllowsSelection = false;
            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.RowHeight = 80;
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            TableView.TableFooterView = new UIView();

            var source = ViewModel.FoundContacts.GetTableViewSource((cell, viewModel, index) =>
            {
                (cell as ChatUserViewCell)?.BindViewModel(viewModel);
            }, ChatUserViewCell.Key);
            TableView.Source = source;

            TableViewSearchBar.TextChanged += (sender, e) =>
            {
                ViewModel.UserNameSearchQuery = e.SearchText;
            };
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            var indexPath = TableView.IndexPathForSelectedRow;
            if (indexPath != null)
            {
                TableView.DeselectRow(indexPath, false);
            }
        }
    }
}

