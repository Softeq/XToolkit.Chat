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

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.HasSelectedItems).WhenSourceChanges(() =>
            {
                SelectedMembersCollectionView.Hidden = !ViewModel.HasSelectedItems;
                //SelectedMembersCollectionViewHeightConstaint.Constant = ViewModel.HasSelectedItems ? 80 : 0;
            }));
        }

        private void InitNavigationBar()
        {
            CustomNavigationBarItem.Title = ViewModel.Resources.AddMembers;

            CustomNavigationBar.ShadowImage = new UIImage();

            CustomNavigationBarItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.BackButtonBundleName),
                ViewModel.CancelCommand,
                true);

            CustomNavigationBarItem.SetCommand(
                ViewModel.Resources.Done,
                StyleHelper.Style.NavigationBarTintColor,
                ViewModel.DoneCommand,
                false);
        }

        private void InitSearchBar()
        {
            TableViewSearchBar.Placeholder = ViewModel.Resources.Search;
            TableViewSearchBar.TextChanged += (sender, e) =>
            {
                ViewModel.UserNameSearchQuery = e.SearchText;
            };
        }

        private void InitSearchMembersTableView()
        {
            TableView.RowHeight = 50;
            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.TableFooterView = new UIView();

            var source = new ObservableTableViewSource<ChatUserViewModel>
            {
                DataSource = ViewModel.FoundContacts,
                BindCellDelegate = (cell, viewModel, index) =>
                {
                    if (cell is ChatUserViewCell memberCell)
                    {
                        memberCell.BindViewModel(viewModel);
                    }
                },
                ReuseId = ChatUserViewCell.Key
            };

            TableView.Source = source;
        }

        private void InitSelectedMembersCollectionView()
        {
            SelectedMembersCollectionView.RegisterNibForCell(SelectedMemberViewCell.Nib, SelectedMemberViewCell.Key);

            var source = new ObservableCollectionViewSource<ChatUserViewModel, SelectedMemberViewCell>
            {
                DataSource = ViewModel.SelectedContacts,
                BindCellDelegate = (cell, viewModel, index) =>
                {
                    cell.BindCell(viewModel);
                },
                ReuseId = SelectedMemberViewCell.Key
            };

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
