// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using Android.Support.V7.Widget;
using Android.Views;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.EventArguments;

namespace Softeq.XToolkit.Chat.Droid.Adapters
{
    public class ConversationsObservableRecyclerViewAdapter
        : GroupedObservableRecyclerViewAdapter<DateTimeOffset, ChatMessageViewModel>
    {
        private const int InfoMessageViewType = 1;
        private const int OutComingMessageViewType = 2;
        private const int InComingMessageViewType = 3;

        private readonly WeakAction<int> _collectionChangedAction;
        private readonly WeakAction<int> _lastItemsLoadedAction;
        private readonly WeakFunc<DateTimeOffset, string> _headerGroupConverter;
        private readonly string[] _contextMenuOptions;

        public event EventHandler LastItemRequested;

        public ConversationsObservableRecyclerViewAdapter(
            ObservableKeyGroupsCollection<DateTimeOffset, ChatMessageViewModel> items,
            Action<int> collectionChangedAction,
            Action<int> lastItemsLoadedAction,
            Func<DateTimeOffset, string> headerGroupConverter,
            string[] contextMenuOptions)
            : base(items, null)
        {
            _collectionChangedAction = new WeakAction<int>(collectionChangedAction);
            _lastItemsLoadedAction = new WeakAction<int>(lastItemsLoadedAction);
            _headerGroupConverter = new WeakFunc<DateTimeOffset, string>(headerGroupConverter);
            _contextMenuOptions = contextMenuOptions;
        }

        public override int GetItemViewType(int position)
        {
            var data = DataSource[position];

            if (data.ItemType == GroupItemTypes.Data)
            {
                if (((GroupDataItem<ChatMessageViewModel>)data).Data.IsMine)
                {
                    return OutComingMessageViewType;
                }
                return InComingMessageViewType;
            }

            return InfoMessageViewType;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch (viewType)
            {
                case InfoMessageViewType:
                    return new ConversationInfoViewHolder(LayoutInflater.From(parent.Context)
                        .Inflate(Resource.Layout.item_conversation_info, parent, false), CreateHeaderModel);
                case InComingMessageViewType:
                    return new ConversationViewHolder(LayoutInflater.From(parent.Context)
                        .Inflate(Resource.Layout.item_conversation_incoming, parent, false), true, _contextMenuOptions);
                case OutComingMessageViewType:
                    return new ConversationViewHolder(LayoutInflater.From(parent.Context)
                        .Inflate(Resource.Layout.item_conversation_outcoming, parent, false), false, _contextMenuOptions);
                default:
                    throw new InvalidEnumArgumentException(nameof(viewType), viewType, typeof(int));
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (position == 0)
            {
                TryToRaiseLastItemRequest();
            }

            base.OnBindViewHolder(holder, position);
        }

        protected override void NotifyCollectionChanged(NotifyKeyGroupsCollectionChangedEventArgs e)
        {
            HandleModifiedItemsForLastItemRequest(e);

            HandleModifiedItemsForFooterOfCollection(e);
        }

        private string CreateHeaderModel(DateTimeOffset key)
        {
            return _headerGroupConverter.Execute(key);
        }

        private void TryToRaiseLastItemRequest()
        {
            LastItemRequested?.Invoke(this, EventArgs.Empty);
        }

        private void HandleModifiedItemsForLastItemRequest(NotifyKeyGroupsCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }
            var lastModifiedItemsCount = 0;
            foreach (var newSectionIndex in e.ModifiedSectionsIndexes)
            {
                lastModifiedItemsCount += GroupedDataSource[newSectionIndex].Count + 1;
            }
            foreach (var (section, modifiedIndexes) in e.ModifiedItemsIndexes)
            {
                lastModifiedItemsCount += modifiedIndexes.Count;
            }

            if (lastModifiedItemsCount > 0)
            {
                if (_lastItemsLoadedAction.IsAlive)
                {
                    _lastItemsLoadedAction.Execute(lastModifiedItemsCount);
                }
            }
        }

        private void HandleModifiedItemsForFooterOfCollection(NotifyKeyGroupsCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || !_collectionChangedAction.IsAlive)
            {
                return;
            }

            var lastDataSourceItemIndex = ItemCount - 1;
            var lastGroupedDataSourceItemIndex = GroupedDataSource.Count - 1;

            var lastInsertedSectionIndex = int.MinValue;
            if (e.ModifiedSectionsIndexes.Any())
            {
                lastInsertedSectionIndex = e.ModifiedSectionsIndexes.Max();
            }

            if (lastInsertedSectionIndex == lastGroupedDataSourceItemIndex)
            {
                _collectionChangedAction.Execute(lastDataSourceItemIndex);
            }
            else
            {
                var (section, modifiedIndexes) = e.ModifiedItemsIndexes.FirstOrDefault(x => x.Section == lastGroupedDataSourceItemIndex);

                if (modifiedIndexes != null && modifiedIndexes.Any())
                {
                    var lastInsertedItemIndex = modifiedIndexes.Max();
                    if (lastInsertedItemIndex == GroupedDataSource[section].Count - 1)
                    {
                        _collectionChangedAction.Execute(lastDataSourceItemIndex);
                    }
                }
            }
        }
    }
}
