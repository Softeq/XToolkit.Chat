// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.ComponentModel;
using AsyncDisplayKitBindings;
using CoreGraphics;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.TableSources
{
    public class GroupedTableDelegate : ASTableDelegate, INotifyPropertyChanged
    {
        private readonly bool _isInverted;
        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<ASBatchContext> MoreDataRequested;

        public GroupedTableDelegate(bool isInverted = false)
        {
            _isInverted = isInverted;
        }

        public Func<UITableView, nint, UIView> GetViewForHeaderDelegate { get; set; }
        public nfloat SectionHeaderHeight { get; set; } = 1f;
        public Func<UITableView, nint, UIView> GetViewForFooterDelegate { get; set; }
        public nfloat SectionFooterHeight { get; set; } = 1f;

        public override nfloat HeightForHeaderInSection(UITableView tableView, nint section)
        {
            if (!_isInverted && GetViewForHeaderDelegate != null)
            {
                return SectionHeaderHeight;
            }
            if (_isInverted && GetViewForFooterDelegate != null)
            {
                return SectionFooterHeight;
            }
            return 1;
        }

        public override UIView ViewForHeaderInSection(UITableView tableView, nint section)
        {
            if (!_isInverted && GetViewForHeaderDelegate != null)
            {
                return GetViewForHeaderDelegate(tableView, section);
            }
            if (_isInverted && GetViewForFooterDelegate != null)
            {
                var view = GetViewForFooterDelegate(tableView, section);
                view.Transform = CGAffineTransform.MakeScale(1, -1);
                return view;
            }
            return new UIView();
        }

        public override nfloat HeightForFooterInSection(UITableView tableView, nint section)
        {
            if (!_isInverted && GetViewForFooterDelegate != null)
            {
                return SectionFooterHeight;
            }
            if (_isInverted && GetViewForHeaderDelegate != null)
            {
                return SectionHeaderHeight;
            }
            return 1;
        }

        public override UIView ViewForFooterInSection(UITableView tableView, nint section)
        {
            if (!_isInverted && GetViewForFooterDelegate != null)
            {
                return GetViewForFooterDelegate(tableView, section);
            }
            if (_isInverted && GetViewForHeaderDelegate != null)
            {
                var view = GetViewForHeaderDelegate(tableView, section);
                view.Transform = CGAffineTransform.MakeScale(1, -1);
                return view;
            }
            return new UIView();
        }

        public override void WillBeginBatchFetchWithContext(ASTableNode tableNode, ASBatchContext context)
        {
            context.BeginBatchFetching();
            MoreDataRequested?.Invoke(context);
        }

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
