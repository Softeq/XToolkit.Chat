// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Linq;
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
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    //TODO VPY: hard refactoring needed
    public partial class ChatMessagesViewController : ViewControllerBase<ChatMessagesViewModel>
    {
        private const int DefaultScrollDownBottomConstraintValue = 8;
        private const int MinCellHeight = 50;
        private const string ContentSizeKey = "contentSize";

        protected readonly ASTableNode TableNode = new ASTableNode(UITableViewStyle.Grouped);

        private WeakReferenceEx<GroupedTableDataSource<DateTimeOffset, ChatMessageViewModel>> _dataSourceRef;
        private WeakReferenceEx<MessagesTableDelegate> _tableDelegateRef;
        private bool _isAutoScrollAvailable;
        private bool _isAutoScrollToBottomEnabled = true;
        private bool _shouldUpdateTableViewContentOffset;
        private NSLayoutConstraint _tableViewBottomConstraint;
        private ContextMenuComponent _contextMenuComponent;
        private ConnectionStatusView _customTitleView;

        public ChatMessagesViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupInputAccessoryView();

            _customTitleView = new ConnectionStatusView(CGRect.Empty);

            CustomNavigationItem.AddTitleView(_customTitleView);
            CustomNavigationItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.BackButtonBundleName),
                ViewModel.BackCommand,
                true);
            CustomNavigationItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.ChatDetailsButtonBundleName),
                ViewModel.ShowInfoCommand,
                false);

            InitTableViewAsync().SafeTaskWrapper();

            _contextMenuComponent = new ContextMenuComponent();
            _contextMenuComponent.AddCommand(ContextMenuActions.Edit, ViewModel.MessageCommandActions[0]);
            _contextMenuComponent.AddCommand(ContextMenuActions.Delete, ViewModel.MessageCommandActions[1]);

            UIMenuController.SharedMenuController.MenuItems = _contextMenuComponent.BuildMenuItems();
            UIMenuController.SharedMenuController.Update();

            InputBar.ViewDidLoad(this);

            InputBar.SetCommandWithArgs(nameof(InputBar.SendRaised), ViewModel.SendCommand);
            InputBar.SetCommand(nameof(InputBar.PickerWillOpen), new RelayCommand(UnregisterKeyboardObservers));
            InputBar.EditingClose.SetCommand(ViewModel.CancelEditingMessageModeCommand);
            InputBar.EditingClose.SetImage(UIImage.FromBundle(StyleHelper.Style.CloseButtonImageBoundleName), UIControlState.Normal);
            InputBar.SetEditLabel(ViewModel.EditMessageLabelText);

            ScrollToBottomButton.SetCommand(new RelayCommand(() => ScrollToBottom(true)));
            ScrollToBottomButton.SetBackgroundImage(UIImage.FromBundle(StyleHelper.Style.ScrollDownBoundleName), UIControlState.Normal);
        }

        public override void ViewWillAppear(bool animated)
        {
            RegisterKeyboardObservers();

            base.ViewWillAppear(animated);

            ViewModel.MessageAddedCommand = new RelayCommand(OnMessageAdded);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            if (SafeAreaBottomOffset > 0)
            {
                InputBar.InvalidateIntrinsicContentSize();
            }

            if (_tableDelegateRef.Target != null)
            {
                _tableDelegateRef.Target.ScrollPositionChanged += TableViewDelegateScrollPositionChanged;
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            if (_isKeyboardOpened)
            {
                HideKeyboard();
            }

            ViewModel.MessageAddedCommand = null;

            if (!NavigationController.ViewControllers.Contains(this))
            {
                _dataSourceRef.Target?.UnsubscribeItemsChanged();
            }

            UnregisterKeyboardObservers();

            if (_tableDelegateRef.Target != null)
            {
                _tableDelegateRef.Target.ScrollPositionChanged -= TableViewDelegateScrollPositionChanged;
            }
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.MessageToSendBody, () => InputBar.Input.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.IsInEditMessageMode).WhenSourceChanges(() =>
            {
                if (ViewModel.IsInEditMessageMode)
                {
                    InputBar.StartEditing(ViewModel.EditedMessageOriginalBody);
                }
                InputBar.ChangeEditingMode(ViewModel.IsInEditMessageMode);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.Messages, () => _dataSourceRef.Target.DataSource));
            Bindings.Add(this.SetBinding(() => ViewModel.ConnectionStatusViewModel.ConnectionStatusText).WhenSourceChanges(() =>
            {
                _customTitleView.Update(ViewModel.ConnectionStatusViewModel);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.MessageToSendBody).WhenSourceChanges(() =>
            {
                InputBar.SetTextPlaceholdervisibility(string.IsNullOrEmpty(ViewModel.MessageToSendBody));
            }));
        }

        public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
        {
            if (keyPath != ContentSizeKey)
            {
                base.ObserveValue(keyPath, ofObject, change, context);
                return;
            }

            var oldSize = ((NSValue)new NSObservedChange(change).OldValue).CGSizeValue;
            var newSize = ((NSValue)new NSObservedChange(change).NewValue).CGSizeValue;

            if (ofObject is ASTableView)
            {
                TableViewContentSizeChanged(newSize, oldSize);
                return;
            }
        }

        private async Task InitTableViewAsync()
        {
            // delay is need to delay UI thread freezing while TableNode items are loaded
            // thus previous screen will not be frozen
            await Task.Delay(1);

            TableNode.View.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            TableNode.View.AddGestureRecognizer(new UITapGestureRecognizer(HideKeyboard));
            TableNode.View.TranslatesAutoresizingMaskIntoConstraints = false;
            TableNode.View.BackgroundColor = UIColor.FromRGB(245, 245, 245);
            MainView.InsertSubview(TableNode.View, 1);

            _tableViewBottomConstraint = TableNode.View.BottomAnchor.ConstraintEqualTo(MainView.BottomAnchor, -InputBarHeight);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                TableNode.View.RightAnchor.ConstraintEqualTo(MainView.RightAnchor),
                TableNode.View.LeftAnchor.ConstraintEqualTo(MainView.LeftAnchor),
                TableNode.View.TopAnchor.ConstraintEqualTo(MainView.SafeAreaLayoutGuide.TopAnchor),
                _tableViewBottomConstraint
            });

            TableNode.View.RegisterNibForHeaderFooterViewReuse(MessagesDateHeaderViewCell.Nib, MessagesDateHeaderViewCell.Key);

            TableNode.Inverted = true;
            var tableSource = new GroupedTableDataSource<DateTimeOffset, ChatMessageViewModel>(
                ViewModel.Messages,
                TableNode.View,
                viewModel => new ChatMessageNode(viewModel, _contextMenuComponent),
                TableNode.Inverted);
            var tableDelegate = new MessagesTableDelegate(TableNode.Inverted)
            {
                SectionHeaderHeight = 35f,
                GetViewForHeaderDelegate = GetHeader
            };
            tableDelegate.MoreDataRequested += OnMoreDataRequested;
            TableNode.ShouldAnimateSizeChanges = false;
            TableNode.LayerBacked = true;
            TableNode.RasterizationScale = 1;

            TableNode.View.EstimatedRowHeight = MinCellHeight;
            TableNode.View.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            _dataSourceRef = WeakReferenceEx.Create(tableSource);
            _tableDelegateRef = WeakReferenceEx.Create(tableDelegate);
            TableNode.DataSource = tableSource;
            TableNode.Delegate = tableDelegate;
            TableNode.View.AddObserver(this, ContentSizeKey, NSKeyValueObservingOptions.OldNew, IntPtr.Zero);
            TableNode.SetTuningParameters(new ASRangeTuningParameters { leadingBufferScreenfuls = 1, trailingBufferScreenfuls = 1 }, ASLayoutRangeType.Display);
            TableNode.SetTuningParameters(new ASRangeTuningParameters { leadingBufferScreenfuls = 1, trailingBufferScreenfuls = 1 }, ASLayoutRangeType.Preload);
            TableNode.ReloadData();
            TableNode.LeadingScreensForBatching = 3;
        }

        private UIView GetHeader(UITableView tableView, nint section)
        {
            var sectionFooter = (MessagesDateHeaderViewCell)tableView.DequeueReusableHeaderFooterView(MessagesDateHeaderViewCell.Key);
            if (section < ViewModel.Messages.Count)
            {
                var adjustedSectionIndex = TableNode.Inverted ? ViewModel.Messages.Count - 1 - section : section;
                var date = ViewModel.Messages[(int)adjustedSectionIndex].Key;
                sectionFooter.Title = ViewModel.GetDateString(date);
            }
            return sectionFooter;
        }

        private async void OnMoreDataRequested(ASBatchContext obj)
        {
            await ViewModel.LoadOlderMessagesAsync();
            obj.CompleteBatchFetching(true);
        }

        private void TableViewContentSizeChanged(CGSize newSize, CGSize oldSize)
        {
            var deltaHeight = newSize.Height - oldSize.Height;

            if (!_isAutoScrollToBottomEnabled && _shouldUpdateTableViewContentOffset && deltaHeight > MinCellHeight)
            {
                _shouldUpdateTableViewContentOffset = false;

                TableNode.SetContentOffset(new CGPoint(0, TableNode.ContentOffset.Y + deltaHeight), false);
            }

            //Console.WriteLine($"XXX: {newSize.Height} - {oldSize.Height} = {deltaHeight}, isAutoScroll:{_isAutoScrollToBottomEnabled}");
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

            UpdateScrollDownButtonPosition();
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

        private void UpdateScrollDownButtonPosition()
        {
            if (InputBar.Superview == null || View.Superview == null)
            {
                return;
            }

            var newScrollDownBottomOffset = View.Superview.Frame.Height - InputBar.Superview.Frame.Y;
            if (newScrollDownBottomOffset < InputBarHeight)
            {
                newScrollDownBottomOffset += SafeAreaBottomOffset;
            }
            ScrollDownBottomConstraint.Constant = newScrollDownBottomOffset + DefaultScrollDownBottomConstraintValue;
        }

        private void UpdateScrollDownButtonPositionWithAnimation()
        {
            UIView.Animate(0.1, () =>
            {
                UpdateScrollDownButtonPosition();
                MainView.LayoutIfNeeded();
            });
        }

        class MessagesTableDelegate : GroupedTableDelegate
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
