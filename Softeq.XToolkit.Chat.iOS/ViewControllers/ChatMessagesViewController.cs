// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Threading.Tasks;
using AsyncDisplayKitBindings;
using CoreGraphics;
using Foundation;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.iOS.Controls;
using Softeq.XToolkit.Chat.iOS.TableSources;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.Threading;
using System.Linq;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class ChatMessagesViewController : ViewControllerBase<ChatMessagesViewModel>
    {
        private const int MinCellHeight = 50;
        private const string ContentSizeKey = "contentSize";

        private WeakReferenceEx<GroupedTableDataSource<DateTimeOffset, ChatMessageViewModel>> _dataSourceRef;
        private bool _isAutoScrollAvailable;
        private bool _isAutoScrollToBottomEnabled = true;
        private bool _shouldUpdateTableViewContentOffset;
        private ContextMenuComponent _contextMenuComponent;
        private ConnectionStatusView _customTitleView;
        private MessagesTableDelegate _tableDelegate;
        private WeakReferenceEx<ChatInputKeyboardDelegate> _chatInputDelegate;
        private NSLayoutConstraint _scrollToBottomConstraint;
        private NSLayoutConstraint _tableBottomConstraint;

        public ChatMessagesViewController(IntPtr handle) : base(handle) { }

        protected ASTableNode TableNode { get; private set; }

        protected ChatInputView Input { get; private set; }

        public override UIView InputAccessoryView => Input;

        public override bool CanBecomeFirstResponder => true;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _customTitleView = new ConnectionStatusView(CGRect.Empty);

            TableNode = new ASTableNode(UITableViewStyle.Grouped);
            Input = new ChatInputView() { TranslatesAutoresizingMaskIntoConstraints = false };
            Input.Init(this);
            _chatInputDelegate = new WeakReferenceEx<ChatInputKeyboardDelegate>(Input.Delegate);

            CustomNavigationItem.AddTitleView(_customTitleView);
            CustomNavigationItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.BackButtonBundleName),
                ViewModel.BackCommand,
                true);

            if (ViewModel.HasInfo)
            {
                CustomNavigationItem.SetCommand(
                    UIImage.FromBundle(StyleHelper.Style.ChatDetailsButtonBundleName),
                    ViewModel.ShowInfoCommand,
                    false);
            }

            MainView.InsertSubview(TableNode.View, 1);
            InitTableView();

            _contextMenuComponent = new ContextMenuComponent();
            _contextMenuComponent.AddCommand(ContextMenuActions.Edit, ViewModel.MessageCommandActions[0]);
            _contextMenuComponent.AddCommand(ContextMenuActions.Delete, ViewModel.MessageCommandActions[1]);

            UIMenuController.SharedMenuController.MenuItems = _contextMenuComponent.BuildMenuItems();
            UIMenuController.SharedMenuController.Update();

            Input.SetCommandWithArgs(nameof(Input.SendRaised), ViewModel.MessageInput.SendMessageCommand);
            Input.EditingCloseButton.SetCommand(ViewModel.MessageInput.CancelEditingCommand);
            Input.SetLabels(
                ViewModel.MessageInput.EditMessageHeaderString,
                ViewModel.MessageInput.EnterMessagePlaceholderString);

            ScrollToBottomButton.SetCommand(new RelayCommand(() => ScrollToBottom(true)));
            ScrollToBottomButton.SetBackgroundImage(UIImage.FromBundle(StyleHelper.Style.ScrollDownBoundleName), UIControlState.Normal);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if(IsMovingToParentViewController)
            {
                _chatInputDelegate.Target.KeyboardHeightChanged += Target_KeyboardHeightChanged;
                _tableBottomConstraint.Constant = -Input.IntrinsicContentSize.Height;
            }

            ViewModel.MessageAddedCommand = new RelayCommand(OnMessageAdded);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            _tableDelegate.ScrollPositionChanged += TableViewDelegateScrollPositionChanged;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            ViewModel.MessageAddedCommand = null;

            if (IsMovingFromParentViewController)
            {
                _dataSourceRef.Target?.UnsubscribeItemsChanged();
                _chatInputDelegate.Target.KeyboardHeightChanged -= Target_KeyboardHeightChanged;
            }

            _tableDelegate.ScrollPositionChanged -= TableViewDelegateScrollPositionChanged;
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.MessageInput.MessageBody, () => Input.TextView.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.MessageInput.IsInEditMessageMode).WhenSourceChanges(() =>
            {
                if (ViewModel.MessageInput.IsInEditMessageMode)
                {
                    Input.StartEditing(ViewModel.MessageInput.EditedMessageOriginalBody);
                }
                Input.ChangeEditingMode(ViewModel.MessageInput.IsInEditMessageMode);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.MessagesList.Messages, () => _dataSourceRef.Target.DataSource));
            Bindings.Add(this.SetBinding(() => ViewModel.ConnectionStatus.ConnectionStatusText).WhenSourceChanges(() =>
            {
                _customTitleView.Update(ViewModel.ConnectionStatus);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.MessageInput.MessageBody).WhenSourceChanges(() =>
            {
                Input.SetTextPlaceholdervisibility(string.IsNullOrEmpty(ViewModel.MessageInput.MessageBody));
            }));
            Input.AttachBindings();
        }

        protected override void DoDetachBindings()
        {
            base.DoDetachBindings();
            Input.DetachBindings();
        }

        private void InitTableView()
        {
            TableNode.View.RegisterNibForHeaderFooterViewReuse(MessagesDateHeaderViewCell.Nib, MessagesDateHeaderViewCell.Key);

            MainView.InsertSubview(TableNode.View, 1);

            TableNode.Inverted = true;
            TableNode.ShouldAnimateSizeChanges = false;
            TableNode.LayerBacked = true;
            TableNode.RasterizationScale = 1;

            InitTableViewDelegate();

            InitTableViewAsync().SafeTaskWrapper();

            _scrollToBottomConstraint = ScrollToBottomButton.BottomAnchor.ConstraintEqualTo(TableNode.View.BottomAnchor);
            _scrollToBottomConstraint.Constant = -16;

            _tableBottomConstraint = TableNode.View.BottomAnchor.ConstraintEqualTo(MainView.BottomAnchor, -Input.IntrinsicContentSize.Height);
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                TableNode.View.RightAnchor.ConstraintEqualTo(MainView.RightAnchor),
                TableNode.View.LeftAnchor.ConstraintEqualTo(MainView.LeftAnchor),
                TableNode.View.TopAnchor.ConstraintEqualTo(MainView.SafeAreaLayoutGuide.TopAnchor),
                _tableBottomConstraint,
                _scrollToBottomConstraint
            });
        }

        private void InitTableViewDelegate()
        {
            _tableDelegate = new MessagesTableDelegate(TableNode.Inverted)
            {
                SectionHeaderHeight = 35f,
                GetViewForHeaderDelegate = GetHeader
            };
            _tableDelegate.WillDisplayCell += OnWillDisplayCell;
        }

        private async Task InitTableViewAsync()
        {
            // delay is need to delay UI thread freezing while TableNode items are loaded
            // thus previous screen will not be frozen
            await Task.Delay(1);

            TableNode.View.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            TableNode.View.TranslatesAutoresizingMaskIntoConstraints = false;
            TableNode.View.BackgroundColor = UIColor.FromRGB(245, 245, 245);

            var tableSource = new GroupedTableDataSource<DateTimeOffset, ChatMessageViewModel>(
                ViewModel.MessagesList.Messages,
                TableNode.View,
                viewModel => new ChatMessageNode(viewModel, _contextMenuComponent),
                TableNode.Inverted);

            TableNode.View.EstimatedRowHeight = MinCellHeight;
            TableNode.View.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            _dataSourceRef = WeakReferenceEx.Create(tableSource);
            TableNode.DataSource = tableSource;
            TableNode.Delegate = _tableDelegate;
            TableNode.SetTuningParameters(new ASRangeTuningParameters { leadingBufferScreenfuls = 1, trailingBufferScreenfuls = 1 }, ASLayoutRangeType.Display);
            TableNode.SetTuningParameters(new ASRangeTuningParameters { leadingBufferScreenfuls = 1, trailingBufferScreenfuls = 1 }, ASLayoutRangeType.Preload);
            TableNode.ReloadData();
            TableNode.LeadingScreensForBatching = 3;
        }

        private UIView GetHeader(UITableView tableView, nint section)
        {
            var sectionFooter = (MessagesDateHeaderViewCell)tableView.DequeueReusableHeaderFooterView(MessagesDateHeaderViewCell.Key);
            if (section < ViewModel.MessagesList.Messages.Count)
            {
                var adjustedSectionIndex = TableNode.Inverted ? ViewModel.MessagesList.Messages.Count - 1 - section : section;
                var date = ViewModel.MessagesList.Messages[(int)adjustedSectionIndex].Key;
                sectionFooter.Title = ViewModel.GetDateString(date);
            }
            return sectionFooter;
        }

        private void OnWillDisplayCell(int row)
        {
            if (row == ViewModel.MessagesList.Messages.SelectMany(x => x).Count() - 1)
            {
                ViewModel.MessagesList.LoadOlderMessagesAsync().SafeTaskWrapper();
            }
        }

        private void TableViewDelegateScrollPositionChanged(object sender, nfloat scrollPosition)
        {
            TableViewScrollChanged(scrollPosition);
        }

        private void TableViewScrollChanged(nfloat scrollViewOffsetY)
        {
            var isAutoScrollAvailable = scrollViewOffsetY < TableNode.View.Frame.Height;
            if (isAutoScrollAvailable != _isAutoScrollAvailable)
            {
                _isAutoScrollAvailable = isAutoScrollAvailable;

                UIView.Transition(ScrollToBottomButton, 0.4,
                              UIViewAnimationOptions.TransitionCrossDissolve,
                              () => ScrollToBottomButton.Hidden = isAutoScrollAvailable,
                              () => { });

                _isAutoScrollToBottomEnabled = isAutoScrollAvailable;
            }
        }

        private void OnMessageAdded()
        {
            _shouldUpdateTableViewContentOffset = true;

            if (_isAutoScrollToBottomEnabled)
            {
                ScrollToBottom();
            }
        }

        private void ScrollToBottom(bool isAnimated = false)
        {
            Execute.BeginOnUIThread(() =>
            {
                var index = NSIndexPath.FromRowSection(0, 0);
                TableNode.ScrollToRowAtIndexPath(index, UITableViewScrollPosition.None, isAnimated);
            });
        }

        private void Target_KeyboardHeightChanged(nfloat height)
        {
            var val = height + _tableBottomConstraint.Constant;
            if(val > -_tableBottomConstraint.Constant)
            {
                TableNode.View.ContentOffset = new CGPoint(0, -val);
            }
            TableNode.View.ContentInset = new UIEdgeInsets(val, 0, 0, 0);
            TableNode.View.ScrollIndicatorInsets = new UIEdgeInsets(val, 0, 0, 0);
        }

        private class MessagesTableDelegate : GroupedTableDelegate
        {
            private GroupedTableDataSource<DateTimeOffset, ChatMessageViewModel> _source;

            public event EventHandler LastItemRequested;
            public event EventHandler<nfloat> ScrollPositionChanged;

            public MessagesTableDelegate(bool isInverted) : base(isInverted)
            {
            }

            public override void WillDisplayNodeForRowAtIndexPath(ASTableView tableView, NSIndexPath indexPath)
            {
                if (_source == null)
                {
                    var table = tableView as ASTableView;
                    _source = table.TableNode.DataSource as GroupedTableDataSource<DateTimeOffset, ChatMessageViewModel>;
                }

                var count = _source.DataSource[0].Count;
                if (indexPath.Section == _source.DataSource.Count - 1 && indexPath.Row == count - 1)
                {
                    LastItemRequested?.Invoke(this, EventArgs.Empty);
                }
            }

            [Export("scrollViewDidScroll:")]
            public void Scrolled(UIScrollView scrollView)
            {
                ScrollPositionChanged?.Invoke(this, scrollView.ContentOffset.Y);
            }
        }
    }
}
