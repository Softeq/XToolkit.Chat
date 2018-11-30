// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class AddContactsViewController : ViewControllerBase<AddContactsViewModel>
    {
        public AddContactsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitSearchBar();
            InitSearchMembersTableView();
            InitSelectedMembersCollectionView();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ResetSelectedRow();
        }

        private void InitNavigationBar()
        {
            CustomNavigationBarItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.CloseButtonImageBoundleName),
                ViewModel.CancelCommand,
                true);

            CustomNavigationBarItem.SetCommand(
                "Done",
                StyleHelper.Style.NavigationBarTintColor,
                ViewModel.DoneCommand,
                false);
        }

        private void InitSearchBar()
        {
            TableViewSearchBar.TextChanged += (sender, e) =>
            {
                ViewModel.UserNameSearchQuery = e.SearchText;
            };
        }

        private void InitSearchMembersTableView()
        {
            TableView.AllowsSelection = false;
            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.RowHeight = 80;
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            TableView.TableFooterView = new UIView();

            var source = ViewModel.FoundContacts.GetTableViewSource((cell, viewModel, index) =>
                {
                    if (cell is ChatUserViewCell memberCell)
                    {
                        memberCell.BindViewModel(viewModel);
                    }
                },
                ChatUserViewCell.Key);

            TableView.Source = source;
        }

        private void InitSelectedMembersCollectionView()
        {
            SelectedMembersCollectionView.RegisterNibForCell(SelectedMemberViewCell.Nib, SelectedMemberViewCell.Key);

            var source = ViewModel.SelectedContacts.GetCollectionViewSource<ChatUserViewModel, SelectedMemberViewCell>(
                (cell, viewModel, index) =>
                {
                    cell.Bind(viewModel, ViewModel.RemoveSelectedMemberCommand);
                },
                null,
                SelectedMemberViewCell.Key.ToString());

            SelectedMembersCollectionView.Source = source;
        }

        private void ResetSelectedRow()
        {
            var indexPath = TableView.IndexPathForSelectedRow;
            if (indexPath != null)
            {
                TableView.DeselectRow(indexPath, false);
            }
        }
    }
}
