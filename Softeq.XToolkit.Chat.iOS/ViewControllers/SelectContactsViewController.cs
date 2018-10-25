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
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class SelectContactsViewController : ViewControllerBase<SelectContactsViewModel>
    {
        private const string TempChatPhotoUrl = "https://cdn.pixabay.com/photo/2015/10/23/17/03/eye-1003315_960_720.jpg";

        private WeakReferenceEx<ObservableTableViewSource<ChatUserViewModel>> _sourceRef;

        private ChatDetailsHeaderView _chatDetailsHeaderView;

        public SelectContactsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CustomNavigationItem.SetCommand(UIImage.FromBundle(Styles.BackButtonBundleName), ViewModel.BackCommand, true);
            CustomNavigationItem.SetCommand(ViewModel.ActionButtonName, Styles.NavigationBarTintColor,
                ViewModel.AddChatCommand, false);

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
                ChatAvatarUrl = TempChatPhotoUrl,
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

            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatDetailsHeaderView.ChatNameField.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.IsInviteToChat).WhenSourceChanges(() =>
            {
                if (ViewModel.IsInviteToChat)
                {
                    _chatDetailsHeaderView.RemoveFromSuperview();
                    TableView.TableHeaderView = new UIView();
                }

                CustomNavigationItem.Title = ViewModel.Title;
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.ContactsCountText, () => _chatDetailsHeaderView.ChatMembersCount));
        }
    }
}

