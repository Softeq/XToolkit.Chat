// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Weak;

namespace Softeq.XToolkit.Chat.Droid.Adapters
{
    public class BaseChatObservableRecyclerViewAdapter<T> : RecyclerView.Adapter
    {
        private readonly List<IBindableViewHolder<T>> _viewHolders = new List<IBindableViewHolder<T>>();
        private readonly WeakFunc<(ViewGroup, int), BindableViewHolder<T>> _getHolderFunc;

        private IList<T> _dataSource;
        private IDisposable _subscription;

        public BaseChatObservableRecyclerViewAdapter(
            IList<T> items,
            Func<(ViewGroup, int), BindableViewHolder<T>> getHolderFunc)
        {
            if (getHolderFunc != null)
            {
                _getHolderFunc = new WeakFunc<(ViewGroup, int), BindableViewHolder<T>>(getHolderFunc);
            }

            DataSource = items;
        }

        public override int ItemCount => _dataSource.Count;

        private IList<T> DataSource
        {
            get => _dataSource;
            set
            {
                if (Equals(_dataSource, value))
                {
                    return;
                }

                _dataSource = value;

                if (_dataSource is INotifyCollectionChanged notifier)
                {
                    _subscription = new NotifyCollectionChangedEventSubscription(notifier, NotifierCollectionChanged);
                }
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = DataSource[position];

            if (holder is IBindableViewHolder<T> viewHolder)
            {
                viewHolder.UnbindViewModel();
                viewHolder.BindViewModel(item);

                // TODO: improve duplicates by position
                _viewHolders.Add(viewHolder);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscription?.Dispose();

                _viewHolders.Apply(x =>
                {
                    x.UnbindViewModel();
                    x.Dispose();
                });
                _viewHolders.Clear();
            }

            base.Dispose(disposing);
        }

        protected virtual void NotifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedOnMainThread(e);
        }

        private void NotifyCollectionChangedOnMainThread(NotifyCollectionChangedEventArgs e)
        {
            if (Looper.MainLooper == Looper.MyLooper())
            {
                NotifyCollectionChangedByAction(e);
            }
            else
            {
                var h = new Handler(Looper.MainLooper);
                h.Post(() => NotifyCollectionChangedByAction(e));
            }
        }

        protected virtual void NotifyCollectionChangedByAction(NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        NotifyItemRangeInserted(e.NewStartingIndex, e.NewItems.Count);
                        break;
                    case NotifyCollectionChangedAction.Move:
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            NotifyItemMoved(e.OldStartingIndex + i, e.NewStartingIndex + i);
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        NotifyItemRangeChanged(e.NewStartingIndex, e.NewItems.Count);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        NotifyItemRangeRemoved(e.OldStartingIndex, e.OldItems.Count);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        NotifyDataSetChanged();
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.Warn(nameof(BaseChatObservableRecyclerViewAdapter<T>),
                            "Exception masked during Adapter RealNotifyDataSetChanged {0}. Are you trying to update your collection from a background task? See http://goo.gl/0nW0L6",
                            exception.ToString());
            }
        }
    }
}
