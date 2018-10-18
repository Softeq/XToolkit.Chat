// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using UIKit;
using CoreGraphics;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class SelectContactsViewController : ViewControllerBase<SelectContactsViewModel>
    {
        private WeakReferenceEx<ObservableTableViewSource<ChatUserViewModel>> _sourceRef;

        private UIBarButtonItem _actionButton;
        private ChatDetailsHeaderView _chatDetailsHeaderView;

        public SelectContactsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _actionButton = new UIBarButtonItem();
            _actionButton.SetCommand(ViewModel.AddChatCommand);
            NavigationItem.RightBarButtonItem = _actionButton;

            TableView.AllowsSelection = false;
            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.RowHeight = 80;
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            TableView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _chatDetailsHeaderView.ChatNameField.ResignFirstResponder();
            }));

            _chatDetailsHeaderView = new ChatDetailsHeaderView(new CGRect(0, 0, 200, 220))
            {
                IsEditMode = true,
                IsAddMembersButtonHidden = true,
                ChatAvatarUrl = "https://cdn.pixabay.com/photo/2015/10/23/17/03/eye-1003315_960_720.jpg",
            };
            TableView.TableHeaderView = _chatDetailsHeaderView;
            TableView.TableFooterView = new UIView();

            var source = ViewModel.Contacts.GetTableViewSource((cell, viewModel, index) =>
            {
                (cell as ChatUserViewCell).BindViewModel(viewModel);
            }, ChatUserViewCell.Key);
            TableView.Source = source;
            _sourceRef = WeakReferenceEx.Create(source);
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

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ActionButtonName, () => _actionButton.Title));
            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatDetailsHeaderView.ChatNameField.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.IsInviteToChat).WhenSourceChanges(() =>
            {
                if (ViewModel.IsInviteToChat)
                {
                    _chatDetailsHeaderView.RemoveFromSuperview();
                    TableView.TableHeaderView = new UIView();
                }

                Title = ViewModel.IsInviteToChat ? "Invite users" : "Create group";
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.ContactsCountText, () => _chatDetailsHeaderView.ChatMembersCount));
        }
    }
}

