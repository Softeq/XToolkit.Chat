// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Windows.Input;
using Android.Support.V7.Widget;

namespace Softeq.XToolkit.Chat.Droid.Listeners
{
    public class ScrollDownScrollListener : RecyclerView.OnScrollListener
    {
        private const int VisibleThreshold = 6;

        private readonly ICommand _scrollDownButtonVisibilityChangedCommand;

        public ScrollDownScrollListener(ICommand scrollDownButtonVisibilityChangedCommand)
        {
            _scrollDownButtonVisibilityChangedCommand = scrollDownButtonVisibilityChangedCommand;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            var layoutManager = recyclerView.GetLayoutManager() as LinearLayoutManager;
            if (layoutManager == null)
            {
                throw new ArgumentNullException("LayoutManager");
            }

            var adapter = recyclerView.GetAdapter();
            if (adapter == null)
            {
                throw new ArgumentNullException("Adapter");
            }

            var currentVisibleItemPosition = adapter.ItemCount - layoutManager.FindLastVisibleItemPosition();

            _scrollDownButtonVisibilityChangedCommand.Execute(currentVisibleItemPosition > VisibleThreshold);
        }
    }
}
