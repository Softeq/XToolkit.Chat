// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.Support.V7.Widget;
using Softeq.XToolkit.Common.Tasks;

namespace Softeq.XToolkit.Chat.Droid.Listeners
{
    public class LoadMoreScrollListener : RecyclerView.OnScrollListener
    {
        private const int VisibleThreshold = 5;

        private readonly TaskReference _onScrolled;

        private bool _isBusy;

        public LoadMoreScrollListener(TaskReference onScrolled)
        {
            _onScrolled = onScrolled ?? throw new ArgumentNullException(nameof(onScrolled));
        }

        public override async void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            var linearLayoutManager = (LinearLayoutManager) recyclerView.GetLayoutManager();

            var totalItemCount = linearLayoutManager.ItemCount;
            var lastVisibleItem = linearLayoutManager.FindLastVisibleItemPosition();

            if (!_isBusy && totalItemCount <= (lastVisibleItem + VisibleThreshold))
            {
                _isBusy = true;

                await _onScrolled.RunAsync();

                _isBusy = false;
            }
        }
    }
}
