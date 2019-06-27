// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Softeq.XToolkit.Common.Droid.Extensions;

namespace Softeq.XToolkit.Chat.Droid.Controls
{
    public class LeftOffsetItemDecoration : RecyclerView.ItemDecoration
    {
        private readonly float _leftOffset;
        private readonly float _bottomOffset;
        private readonly Paint _paint;

        public LeftOffsetItemDecoration(Context context, int colorResId, int leftOffsetDpi, int heightDpi = 1)
        {
            _leftOffset = context.ToPixels(leftOffsetDpi);
            _bottomOffset = context.ToPixels(heightDpi);

            _paint = new Paint
            {
                Color = new Color(ContextCompat.GetColor(context, colorResId)),
                StrokeWidth = _bottomOffset
            };
        }

        public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
        {
            var parameters = (RecyclerView.LayoutParams)view.LayoutParameters;
            var position = parameters.ViewAdapterPosition;

            // add a separator to any view but the last one
            if (position < state.ItemCount)
            {
                outRect.Set(0, 0, 0, (int)_bottomOffset);
            }
            else
            {
                outRect.SetEmpty();
            }
        }

        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            var offset = (int)(_bottomOffset / 2);

            // iterate over every visible view
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var view = parent.GetChildAt(i);
                var parameters = (RecyclerView.LayoutParams)view.LayoutParameters;

                var position = parameters.ViewAdapterPosition;

                // finally draw the separator
                if (position < state.ItemCount)
                {
                    c.DrawLine(
                        view.Left + _leftOffset,
                        view.Bottom + offset,
                        view.Right,
                        view.Bottom + offset,
                        _paint);
                }
            }
        }
    }
}
