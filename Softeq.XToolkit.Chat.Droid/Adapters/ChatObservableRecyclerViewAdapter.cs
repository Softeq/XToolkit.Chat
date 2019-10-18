// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.Views;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Weak;

namespace Softeq.XToolkit.Chat.Droid.Adapters
{
    public class ChatObservableRecyclerViewAdapter : BaseChatObservableRecyclerViewAdapter<ChatSummaryViewModel>
    {
        private readonly WeakAction<int> _scrollPositionChangeAction;

        public ChatObservableRecyclerViewAdapter(
            IList<ChatSummaryViewModel> items,
            Func<(ViewGroup, int), BindableViewHolder<ChatSummaryViewModel>> getHolderFunc,
            Action<int> scrollPositionChangeAction)
            : base(items, getHolderFunc)
        {
            _scrollPositionChangeAction = new WeakAction<int>(scrollPositionChangeAction);
        }

        protected override void NotifyCollectionChangedByAction(NotifyCollectionChangedEventArgs e)
        {
            base.NotifyCollectionChangedByAction(e);

            if (e.Action == NotifyCollectionChangedAction.Add &&
                _scrollPositionChangeAction.IsAlive)
            {
                _scrollPositionChangeAction.Execute(0);
            }
        }
    }
}
