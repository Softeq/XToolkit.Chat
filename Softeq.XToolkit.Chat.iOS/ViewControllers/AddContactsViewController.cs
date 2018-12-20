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
        private const float DefaultSelectedMembersCollectionTopConstraint = 20;
        private const float DefaultFoundMembersCellHeight = 50;

        private ObservableTableViewSource<ChatUserViewModel> _tableViewSource;

        public AddContactsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitSearchBar();
            InitSearchMembersTableView();
            InitSelectedMembersCollectionView();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            TableViewSearchBar.TextChanged += TableViewSearchBar_TextChanged;
            _tableViewSource.ItemTapped += TableViewSource_ItemTapped;
            _tableViewSource.LastItemRequested += TableViewSource_LastItemRequested;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            TableViewSearchBar.TextChanged -= TableViewSearchBar_TextChanged;
            _tableViewSource.ItemTapped -= TableViewSource_ItemTapped;
            _tableViewSource.LastItemRequested -= TableViewSource_LastItemRequested;

            ResetSelectedRow();
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.HasSelectedContacts).WhenSourceChanges(() =>
            {
                UIView.Animate(0.5, () =>
                {
                    SelectedMembersCollectionView.Hidden = !ViewModel.HasSelectedContacts;
                    SelectedMembersCollectionViewTopConstraint.Constant = ViewModel.HasSelectedContacts
                        ? DefaultSelectedMembersCollectionTopConstraint + SelectedMembersCollectionView.Frame.Height
                        : DefaultSelectedMembersCollectionTopConstraint;
                    SelectedMembersCollectionView.LayoutIfNeeded();
                });
            }));
        }

        private void TableViewSearchBar_TextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            ViewModel.ContactNameSearchQuery = e.SearchText;
        }

        private void TableViewSource_ItemTapped(object sender, Common.EventArguments.GenericEventArgs<ChatUserViewModel> e)
        {
            e.Value.IsSelected = !e.Value.IsSelected;
        }

        private async void TableViewSource_LastItemRequested(object sender, EventArgs e)
        {
            await ViewModel.PaginationViewModel.LoadNextPageAsync();
        }

        private void InitNavigationBar()
        {
            CustomNavigationBarItem.Title = ViewModel.Title;

            CustomNavigationBar.ShadowImage = new UIImage();

            CustomNavigationBarItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.BackButtonBundleName),
                ViewModel.CancelCommand,
                true);

            CustomNavigationBarItem.SetCommand(
                ViewModel.Resources.Done,
                StyleHelper.Style.AccentColor,
                ViewModel.DoneCommand,
                false);
        }

        private void InitSearchBar()
        {
            TableViewSearchBar.Placeholder = ViewModel.Resources.Search;
        }

        private void InitSearchMembersTableView()
        {
            TableView.RowHeight = DefaultFoundMembersCellHeight;
            TableView.RegisterNibForCellReuse(FilteredContactViewCell.Nib, FilteredContactViewCell.Key);
            TableView.TableFooterView = new UIView();
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;

            _tableViewSource = new ObservableTableViewSource<ChatUserViewModel>
            {
                DataSource = ViewModel.PaginationViewModel.Items,
                BindCellDelegate = (cell, viewModel, index) =>
                {
                    if (cell is FilteredContactViewCell memberCell)
                    {
                        memberCell.BindViewModel(viewModel);
                    }
                },
                ReuseId = FilteredContactViewCell.Key
            };

            TableView.Source = _tableViewSource;
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
