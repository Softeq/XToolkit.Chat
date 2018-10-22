// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using Foundation;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.iOS;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class ChatsListViewController : ViewControllerBase<ChatsListViewModel>
    {
        private const string ReLoginButtonText = "(RE)LOGIN";
        private const string ReLoginButtonFormattedText = "(RE)LOGIN: {0}";

        private WeakReferenceEx<ObservableTableViewSource<ChatSummaryViewModel>> _sourceRef;

        public ChatsListViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            if (ViewModel == null)
            {
                var viewModel = ServiceLocator.Resolve<ChatsListViewModel>();
                SetExistingViewModel(viewModel);
            }

            base.ViewDidLoad();

            // Reset text for back button
            NavigationItem.BackBarButtonItem = new UIBarButtonItem("", UIBarButtonItemStyle.Plain, null, null);
            NavigationItem.TitleView = CustomTitleView;

            // Setup brand color for NavigationBar buttons
            NavigationController.NavigationBar.BarTintColor = UIColor.White;
            NavigationController.NavigationBar.TintColor = UIColor.FromRGB(62, 218, 215);
            NavigationController.NavigationBar.Translucent = false;

            var addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            addButton.SetCommand(ViewModel.CreateChatCommand);
            NavigationItem.RightBarButtonItem = addButton;

            LoginButton.SetCommand(ViewModel.LoginCommand);

            ChatsTableView.RegisterNibForCellReuse(ChatSummaryViewCell.Nib, ChatSummaryViewCell.Key);
            ChatsTableView.RowHeight = 80;
            ChatsTableView.TableFooterView = new UIView();

            var source = ViewModel.Chats.GetTableViewSource((cell, viewModel, index) =>
            {
                (cell as ChatSummaryViewCell).BindViewModel(viewModel);
            }, ChatSummaryViewCell.Key);
            ChatsTableView.Source = source;
            ChatsTableView.Delegate = new CustomViewDelegate(source, ViewModel);
            _sourceRef = WeakReferenceEx.Create(source);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.UserName).WhenSourceChanges(() =>
            {
                var reloginButtonTitle = string.IsNullOrEmpty(ViewModel.UserName)
                    ? ReLoginButtonText
                    : string.Format(ReLoginButtonFormattedText, ViewModel.UserName);
                LoginButton.SetTitle(reloginButtonTitle, UIControlState.Normal);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.SelectedChat, () => _sourceRef.Target.SelectedItem, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.IsReloginButtonVisible, () => LoginButton.Hidden)
                         .ConvertSourceToTarget(x => !x));

            Bindings.Add(this.SetBinding(() => ViewModel.ConnectionStatusViewModel).WhenSourceChanges(() =>
            {
                CustomTitleView.Update(ViewModel.ConnectionStatusViewModel);
            }));
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            var indexPath = ChatsTableView.IndexPathForSelectedRow;
            if (indexPath != null)
            {
                ChatsTableView.DeselectRow(indexPath, false);
            }
        }

        private class CustomViewDelegate : UITableViewDelegate
        {
            private readonly WeakReferenceEx<ObservableTableViewSource<ChatSummaryViewModel>> _sourceRef;
            private readonly WeakReferenceEx<ChatsListViewModel> _viewModelRef;

            public CustomViewDelegate(ObservableTableViewSource<ChatSummaryViewModel> source, ChatsListViewModel viewModel)
            {
                _sourceRef = WeakReferenceEx.Create(source);
                _viewModelRef = WeakReferenceEx.Create(viewModel);
            }

            public CustomViewDelegate(IntPtr handle) : base(handle) { }

            public CustomViewDelegate(NSObjectFlag t) : base(t) { }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                var isCreatedByMe = _sourceRef.Target?.DataSource[indexPath.Row] != null
                                              && _sourceRef.Target.DataSource[indexPath.Row].IsCreatedByMe;
                var buttons = new List<UITableViewRowAction>
                {
                    UITableViewRowAction.Create(
                        UITableViewRowActionStyle.Default,
                        _viewModelRef.Target?.LeaveChatOptionText,
                        (row, index) => OnClickLeave(row, index, tableView))
                };

                if (isCreatedByMe)
                {
                    var closeButton = UITableViewRowAction.Create(
                        UITableViewRowActionStyle.Default,
                        _viewModelRef.Target?.DeleteChatOptionText,
                        (row, index) => OnClickDelete(row, index, tableView));
                    closeButton.BackgroundColor = UIColor.Orange;
                    buttons.Add(closeButton);
                }

                return buttons.ToArray();
            }

            [Export("tableView:didSelectRowAtIndexPath:")]
            public void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                _sourceRef.Target?.RowSelected(tableView, indexPath);
            }

            private void OnClickLeave(UITableViewRowAction row, NSIndexPath indexPath, UITableView tableView)
            {
                tableView.SetEditing(false, true);
                if (_sourceRef.Target == null || indexPath.Row >= _sourceRef.Target.DataSource.Count || indexPath.Row < 0)
                {
                    return;
                }
                var cellViewModel = _sourceRef.Target?.DataSource[indexPath.Row];
                if (cellViewModel == null)
                {
                    return;
                }

                _viewModelRef.Target?.LeaveChatCommand.Execute(cellViewModel);
            }

            private void OnClickDelete(UITableViewRowAction row, NSIndexPath indexPath, UITableView tableView)
            {
                tableView.SetEditing(false, true);
                if (_sourceRef.Target == null || indexPath.Row >= _sourceRef.Target.DataSource.Count || indexPath.Row < 0)
                {
                    return;
                }
                var cellViewModel = _sourceRef.Target?.DataSource[indexPath.Row];
                if (cellViewModel == null)
                {
                    return;
                }

                _viewModelRef.Target?.DeleteChatCommand.Execute(cellViewModel);
            }
        }
    }
}

