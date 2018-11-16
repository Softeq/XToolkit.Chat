// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using UIKit;
using CoreGraphics;
using Foundation;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class ChatDetailsViewController : ViewControllerBase<ChatDetailsViewModel>
    {
        private ChatDetailsHeaderView _chatDetailsHeaderView;
        private WeakReferenceEx<ObservableTableViewSource<ChatUserViewModel>> _sourceRef;

        public ChatDetailsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CustomNavigationItem.Title = ViewModel.Title;
            CustomNavigationItem.SetCommand(UIImage.FromBundle(Styles.BackButtonBundleName), ViewModel.BackCommand, true);

            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.RowHeight = 80;

            _chatDetailsHeaderView = new ChatDetailsHeaderView(new CGRect(0, 0, 200, 250));
            _chatDetailsHeaderView.IsEditMode = false;
            _chatDetailsHeaderView.SetAddMembersCommand(ViewModel.AddMembersCommand);

            TableView.TableHeaderView = _chatDetailsHeaderView;
            TableView.TableFooterView = new UIView();

            var source = ViewModel.Members.GetTableViewSource((cell, viewModel, index) =>
            {
                (cell as ChatUserViewCell)?.BindViewModel(viewModel);
            }, ChatUserViewCell.Key);
            TableView.Source = source;
            TableView.Delegate = new ParticipantsTableViewDelegate(ViewModel);

            _sourceRef = WeakReferenceEx.Create(source);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatAvatarUrl).WhenSourceChanges((() =>
            {
                _chatDetailsHeaderView.SetChatAvatar(ViewModel.ChatAvatarUrl);
            })));
            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatDetailsHeaderView.ChatNameField.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.MembersCountText, () => _chatDetailsHeaderView.ChatMembersCount));
        }

        private class ParticipantsTableViewDelegate : UITableViewDelegate
        {
            private readonly ChatDetailsViewModel _viewModel;

            public ParticipantsTableViewDelegate(ChatDetailsViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (_viewModel.IsEditAvailable && !_viewModel.IsMemberRemovable((int) indexPath.Item))
                {
                    return new UITableViewRowAction[0];
                }

                var remove = UITableViewRowAction.Create(
                    UITableViewRowActionStyle.Default,
                    _viewModel.RemoveLocalizedString,
                    (row, index) => _viewModel.RemoveMemberAtCommand.Execute((int) indexPath.Item));

                return new[] {remove};
            }
        }
    }
}