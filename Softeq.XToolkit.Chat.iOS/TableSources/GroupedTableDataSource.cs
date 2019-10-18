// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using AsyncDisplayKitBindings;
using CoreAnimation;
using Foundation;
using Softeq.XToolkit.Common.Collections;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.TableSources
{
    public class GroupedTableDataSource<TKey, TValue> : ASTableDataSource
    {
        private readonly bool _isInverted;
        private readonly Thread _mainThread;

        private ObservableKeyGroupsCollection<TKey, TValue> _dataSource;
        private UITableView _tableView;
        private bool _isSubscribed;

        public GroupedTableDataSource(ObservableKeyGroupsCollection<TKey, TValue> items,
                                      UITableView tableView,
                                      Func<TValue, ASCellNode> createCellFunc,
                                      bool isInverted = false)
        {
            _tableView = tableView;
            DataSource = items;
            CreateCellDelegate = createCellFunc;
            _isInverted = isInverted;

            _mainThread = Thread.CurrentThread;
        }

        public Func<TValue, ASCellNode> CreateCellDelegate { get; set; }

        public ObservableKeyGroupsCollection<TKey, TValue> DataSource
        {
            get => _dataSource;
            set
            {
                if (Equals(_dataSource, value))
                {
                    return;
                }

                if (_dataSource != null)
                {
                    UnsubscribeItemsChanged();
                }

                _dataSource = value;
                SubscribeItemsChanged();

                if (_tableView != null)
                {
                    _tableView.ReloadData();
                }
            }
        }

        public override nint NumberOfSectionsInTableNode(ASTableNode tableNode)
        {
            return DataSource.Count;
        }

        public override nint NumberOfRowsInSection(ASTableNode tableNode, nint section)
        {
            var index = GetAdjustedIndex(DataSource.Count, (int) section);
            return _dataSource[index].Count;
        }

        public override ASCellNodeBlock NodeBlockForRowAtIndexPath(ASTableNode tableNode, NSIndexPath indexPath)
        {
            if (_isInverted)
            {
                var invertedSection = GetAdjustedIndex(DataSource.Count, indexPath.Section);
                var invertedRow = GetAdjustedIndex(_dataSource[invertedSection].Count, indexPath.Row);
                var invertedIndexPath = NSIndexPath.FromRowSection(invertedRow, invertedSection);
                var viewModel = DataSource[invertedSection][invertedRow];
                return () => TransformCell(CreateCellDelegate?.Invoke(viewModel));
            }
            else
            {
                var viewModel = DataSource[indexPath.Section][indexPath.Row];
                return () => CreateCellDelegate?.Invoke(viewModel);
            }
        }

        public void SubscribeItemsChanged()
        {
            if (!_isSubscribed)
            {
                _isSubscribed = true;
                _dataSource.ItemsChanged += ItemsChanged;
            }
        }

        public void UnsubscribeItemsChanged()
        {
            if (_isSubscribed)
            {
                _isSubscribed = false;
                _dataSource.ItemsChanged -= ItemsChanged;
            }
        }

        private void ItemsChanged(object sender, NotifyKeyGroupsCollectionChangedEventArgs e)
        {
            var handleChanges = new Action(() =>
            {
                if (e.Action != NotifyCollectionChangedAction.Add && e.Action != NotifyCollectionChangedAction.Remove)
                {
                    _tableView.ReloadData();
                    return;
                }

                var modifiedSectionsIndexes = _isInverted && e.Action == NotifyCollectionChangedAction.Add ||
                                              !_isInverted && e.Action == NotifyCollectionChangedAction.Remove
                       ? e.ModifiedSectionsIndexes.OrderByDescending(x => x)
                       : e.ModifiedSectionsIndexes.OrderBy(x => x);
                foreach (var sectionIndex in modifiedSectionsIndexes)
                {
                    var sectionsCount = e.Action == NotifyCollectionChangedAction.Remove ? e.OldSectionsCount : DataSource.Count;
                    var adjustedSectionIndex = GetAdjustedIndex(sectionsCount, sectionIndex);
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        PerformWithoutAnimation(() =>
                        {
                            _tableView.InsertSections(NSIndexSet.FromIndex(adjustedSectionIndex), UITableViewRowAnimation.None);
                        });
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        _tableView.DeleteSections(NSIndexSet.FromIndex(adjustedSectionIndex), UITableViewRowAnimation.None);
                    }
                }

                var modifiedIndexPaths = new List<NSIndexPath>();
                foreach (var (Section, ModifiedIndexes) in e.ModifiedItemsIndexes)
                {
                    var adjustedSectionIndex = GetAdjustedIndex(DataSource.Count, Section);
                    foreach (var insertedItemIndex in ModifiedIndexes)
                    {
                        var sectionLength = e.Action == NotifyCollectionChangedAction.Remove ? e.OldSectionsSizes[Section] : DataSource[Section].Count;
                        var adjustedItemIndex = GetAdjustedIndex(sectionLength, insertedItemIndex);
                        modifiedIndexPaths.Add(NSIndexPath.FromRowSection(adjustedItemIndex, adjustedSectionIndex));
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    PerformWithoutAnimation(() =>
                    {
                        _tableView.InsertRows(modifiedIndexPaths.ToArray(), UITableViewRowAnimation.None);
                    });
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    _tableView.DeleteRows(modifiedIndexPaths.ToArray(), UITableViewRowAnimation.None);
                }
            });

            if (Thread.CurrentThread == _mainThread)
            {
                handleChanges();
            }
            else
            {
                NSOperationQueue.MainQueue.AddOperation(handleChanges);
                NSOperationQueue.MainQueue.WaitUntilAllOperationsAreFinished();
            }
        }

        private void PerformWithoutAnimation(Action action)
        {
            UIView.PerformWithoutAnimation(action);
        }

        private ASCellNode TransformCell(ASCellNode cell)
        {
            cell.Transform = CATransform3D.MakeScale(1, -1, 1);
            return cell;
        }

        private int GetAdjustedIndex(int count, int originalIndex)
        {
            return !_isInverted ? originalIndex : count - originalIndex - 1;
        }
    }
}
