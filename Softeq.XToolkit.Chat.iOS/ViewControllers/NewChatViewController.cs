using System;
using UIKit;
using CoreGraphics;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Chat.iOS.Controls;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.Common.EventArguments;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class NewChatViewController : ViewControllerBase<NewChatViewModel>
    {
        private ObservableTableViewSource<ChatUserViewModel> _tableViewSource;
        private NewGroupView _tableViewHeader;

        public NewChatViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitSearchBar();
            InitTableView();
            InitProgressIndicator();
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

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

            SearchBar.TextChanged += TableViewSearchBarTextChanged;
            _tableViewSource.ItemTapped += TableViewSourceItemTapped;
            _tableViewSource.LastItemRequested += TableViewSourceLastItemRequested;
        }

        protected override void DoDetachBindings()
        {
            base.DoDetachBindings();

            SearchBar.TextChanged -= TableViewSearchBarTextChanged;
            _tableViewSource.ItemTapped -= TableViewSourceItemTapped;
            _tableViewSource.LastItemRequested -= TableViewSourceLastItemRequested;

            ResetSelectedRow();
        }

        private void TableViewSearchBarTextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            ViewModel.SearchQuery = e.SearchText;
        }

        private void TableViewSourceItemTapped(object sender, GenericEventArgs<ChatUserViewModel> e)
        {
            ViewModel.CreatePersonalChatCommand.Execute(e.Value);
        }

        private async void TableViewSourceLastItemRequested(object sender, EventArgs e)
        {
            await ViewModel.PaginationViewModel.LoadNextPageAsync();
        }

        private void InitNavigationBar()
        {
            CustomNavigationBarItem.Title = ViewModel.LocalizedStrings.NewChat;

            CustomNavigationBar.ShadowImage = new UIImage();

            CustomNavigationBarItem.SetCommand(
                ViewModel.LocalizedStrings.Cancel,
                StyleHelper.Style.AccentColor,
                ViewModel.CancelCommand,
                true);
        }

        private void InitSearchBar()
        {
            SearchBar.Placeholder = ViewModel.LocalizedStrings.Search;
        }

        private void InitTableView()
        {
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

            _tableViewHeader = new NewGroupView(new CGRect(0, 0, TableView.Frame.Width, 60));
            _tableViewHeader.SetCommand(ViewModel.CreateGroupChatCommand);
            _tableViewHeader.SetText(ViewModel.LocalizedStrings.NewGroup);

            TableView.TableHeaderView = _tableViewHeader;
            TableView.TableFooterView = new UIView();
            TableView.RegisterNibForCellReuse(FilteredContactViewCell.Nib, FilteredContactViewCell.Key);
            TableView.Source = _tableViewSource;
            TableView.AddGestureRecognizer(new UITapGestureRecognizer((obj) => View.EndEditing(true))
            {
                CancelsTouchesInView = false
            });
        }

        private void InitProgressIndicator()
        {
            ProgressIndicator.Color = StyleHelper.Style.AccentColor;
            ProgressIndicator.HidesWhenStopped = true;
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
