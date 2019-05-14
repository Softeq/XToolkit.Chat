// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Chat.iOS.Controls;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.EventArguments;
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
            ProgressIndicator.Color = StyleHelper.Style.AccentColor;
            ProgressIndicator.HidesWhenStopped = true;
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

            Bindings.Add(this.SetBinding(() => ViewModel.IsBusy).WhenSourceChanges(() =>
            {
                if (ViewModel.IsBusy)
                {
                    ProgressIndicator.StartAnimating();
                }
                else
                {
                    ProgressIndicator.StopAnimating();
                }
            }));

            TableViewSearchBar.TextChanged += TableViewSearchBarTextChanged;
            _tableViewSource.ItemTapped += TableViewSourceItemTapped;
            _tableViewSource.LastItemRequested += TableViewSourceLastItemRequested;
        }

        protected override void DoDetachBindings()
        {
            base.DoDetachBindings();

            TableViewSearchBar.TextChanged -= TableViewSearchBarTextChanged;
            _tableViewSource.ItemTapped -= TableViewSourceItemTapped;
            _tableViewSource.LastItemRequested -= TableViewSourceLastItemRequested;

            ResetSelectedRow();
        }

        private void TableViewSearchBarTextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            ViewModel.ContactNameSearchQuery = e.SearchText;
        }

        private void TableViewSourceItemTapped(object sender, GenericEventArgs<ChatUserViewModel> e)
        {
            e.Value.IsSelected = !e.Value.IsSelected;
        }

        private async void TableViewSourceLastItemRequested(object sender, EventArgs e)
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
                    if (cell is IBindableViewCell<ChatUserViewModel> memberCell)
                    {
                        memberCell.Bind(viewModel);
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
                    cell.Bind(viewModel);
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