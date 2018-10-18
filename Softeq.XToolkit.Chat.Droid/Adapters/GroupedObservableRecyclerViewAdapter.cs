// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.EventArguments;
using Softeq.XToolkit.Common.WeakSubscription;

namespace Softeq.XToolkit.Chat.Droid.Adapters
{
    public abstract class GroupedObservableRecyclerViewAdapter<TKey, TValue> : RecyclerView.Adapter
    {
        private readonly List<IBindableViewHolder> _viewHolders = new List<IBindableViewHolder>();
        private readonly WeakFunc<(ViewGroup, int), BindableViewHolder<TValue>> _getHolderFunc;

        private ObservableKeyGroupsCollection<TKey, TValue> _groupedDataSource;
        private IDisposable _subscription;

        protected GroupedObservableRecyclerViewAdapter(
            ObservableKeyGroupsCollection<TKey, TValue> items,
            Func<(ViewGroup, int),
            BindableViewHolder<TValue>> getHolderFunc)
        {
            if (getHolderFunc != null)
            {
                _getHolderFunc = new WeakFunc<(ViewGroup, int), BindableViewHolder<TValue>>(getHolderFunc);
            }

            DataSource = ConvertGroupedCollectionToList(items);

            GroupedDataSource = items;
        }

        public override int ItemCount => DataSource.Count;

        protected IList<IGroupItem> DataSource { get; private set; }

        protected ObservableKeyGroupsCollection<TKey, TValue> GroupedDataSource
        {
            get => _groupedDataSource;
            private set
            {
                if (value == null || Equals(_groupedDataSource, value))
                {
                    return;
                }

                _groupedDataSource = value;

                _subscription = CreateSubscriptionForDataSourceChanged(value);
            }
        }

        private IDisposable CreateSubscriptionForDataSourceChanged(ObservableKeyGroupsCollection<TKey, TValue> dataSource)
        {
            if (dataSource is ObservableKeyGroupsCollection<TKey, TValue> groupedCollectionSource)
            {
                return new WeakEventSubscription<ObservableKeyGroupsCollection<TKey, TValue>, NotifyKeyGroupsCollectionChangedEventArgs>(
                groupedCollectionSource, nameof(groupedCollectionSource.ItemsChanged), ItemsChanged);
            }
            return null;
        }

        private void ItemsChanged(object sender, NotifyKeyGroupsCollectionChangedEventArgs e)
        {
            DataSource = ConvertGroupedCollectionToList(GroupedDataSource);

            RunOnUI(NotifyDataSetChanged);

            NotifyCollectionChanged(e);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = DataSource[position];

            if (holder is IBindableViewHolder<TValue> dataViewHolder)
            {
                BindModelToViewHolder(dataViewHolder, ((GroupDataItem<TValue>)item).Data);
            }
            else if (item.ItemType == GroupItemTypes.Header &&
                     holder is IBindableViewHolder<TKey> headerViewHolder)
            {
                BindModelToViewHolder(headerViewHolder, ((GroupHeaderItem<TKey>)item).Data);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (_getHolderFunc != null && _getHolderFunc.IsAlive)
            {
                return _getHolderFunc.Execute((parent, viewType));
            }

            return null;
        }

        protected virtual void NotifyCollectionChanged(NotifyKeyGroupsCollectionChangedEventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscription?.Dispose();
                _subscription = null;

                _viewHolders.Apply(x =>
                {
                    x.UnbindViewModel();
                    x.Dispose();
                });
                _viewHolders.Clear();

                DataSource = null;
                GroupedDataSource = null;
            }

            base.Dispose(disposing);
        }

        private void BindModelToViewHolder<T>(IBindableViewHolder<T> viewHolder, T model)
        {
            viewHolder.UnbindViewModel();
            viewHolder.BindViewModel(model);

            // TODO: improve duplicates by position
            _viewHolders.Add(viewHolder);
        }

        private IList<IGroupItem> ConvertGroupedCollectionToList(ObservableKeyGroupsCollection<TKey, TValue> groups)
        {
            var list = new List<IGroupItem>();

            foreach (var group in groups)
            {
                list.Add(new GroupHeaderItem<TKey>
                {
                    Data = group.Key
                });

                list.AddRange(group.Select(message => new GroupDataItem<TValue>
                {
                    Data = message
                }));
            }

            return list;
        }

        private void RunOnUI(Action handler)
        {
            if (Looper.MainLooper == Looper.MyLooper())
            {
                handler();
            }
            else
            {
                var h = new Handler(Looper.MainLooper);
                h.Post(handler);
            }
        }

        // TODO: Export to separate files

        public interface IGroupItem
        {
            GroupItemTypes ItemType { get; }
        }

        public enum GroupItemTypes
        {
            Header,
            Data
        }

        public class GroupHeaderItem<T> : IGroupItem
        {
            public GroupItemTypes ItemType => GroupItemTypes.Header;

            public T Data { get; set; }
        }

        public class GroupDataItem<T> : IGroupItem
        {
            public GroupItemTypes ItemType => GroupItemTypes.Data;

            public T Data { get; set; }
        }
    }
}
