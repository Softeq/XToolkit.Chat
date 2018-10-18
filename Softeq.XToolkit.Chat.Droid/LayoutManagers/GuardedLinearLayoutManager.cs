// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Java.Lang;

namespace Softeq.XToolkit.Chat.Droid.LayoutManagers
{
    public class GuardedLinearLayoutManager : LinearLayoutManager
    {
        public GuardedLinearLayoutManager(Context context) : base(context)
        {
        }

        protected GuardedLinearLayoutManager(IntPtr ptr, JniHandleOwnership transfer) : base(ptr, transfer)
        {
        }

        public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
        {
            try
            {
                base.OnLayoutChildren(recycler, state);
            }
            catch (IndexOutOfBoundsException e)
            {
                Log.Warn(nameof(GuardedLinearLayoutManager),
                         $"Workaround of issue - https://code.google.com/p/android/issues/detail?id=77846#c1 - IndexOutOfBoundsException {e.Message}");
            }
        }
    }
}
