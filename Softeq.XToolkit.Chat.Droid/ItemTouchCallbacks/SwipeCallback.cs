// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Softeq.XToolkit.Common.Weak;

namespace Softeq.XToolkit.Chat.Droid.ItemTouchCallbacks
{
    /// <summary>
    /// ItemTouchHelper callback for LEFT swipe with actions.
    /// </summary>
    public class SwipeCallback : ItemTouchHelper.SimpleCallback
    {
        private const float DefaultSwipeThreshold = 0.5f;

        private readonly GestureDetector _gestureDetector;
        private readonly WeakReferenceEx<RecyclerView> _recyclerViewRef;
        private readonly Action<RecyclerView.ViewHolder, int, ICollection<ISwipeActionView>> _createSwipeActionViews;
        private readonly Dictionary<int, List<ISwipeActionView>> _actionViewsCache = new Dictionary<int, List<ISwipeActionView>>();
        private readonly List<ISwipeActionView> _actionViews = new List<ISwipeActionView>();
        private readonly Queue<int> _recoverQueue = new Queue<int>();

        private int _swipedPosition;
        private float _swipeThreshold = DefaultSwipeThreshold;

        // TODO: add support actions for right swipe
        public SwipeCallback(
            Context context,
            RecyclerView recyclerView,
            Action<RecyclerView.ViewHolder, int, ICollection<ISwipeActionView>> createSwipeActionViews)
            : base(0, ItemTouchHelper.Left)
        {
            _recyclerViewRef = WeakReferenceEx.Create(recyclerView);
            _createSwipeActionViews = createSwipeActionViews;
            _gestureDetector = new GestureDetector(context, new CustomGestureListener(OnSingleTapConfirmedHandler));
            _recyclerViewRef.Target.Touch += RecyclerViewTouchHandler;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_recyclerViewRef.Target != null)
                {
                    _recyclerViewRef.Target.Touch -= RecyclerViewTouchHandler;
                }
            }

            base.Dispose(disposing);
        }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            return false;
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
        {
            var position = viewHolder.AdapterPosition;

            if (_swipedPosition != position)
            {
                _recoverQueue.Enqueue(_swipedPosition);
            }
            _swipedPosition = position;

            _actionViews.Clear();

            if (_actionViewsCache.ContainsKey(_swipedPosition))
            {
                _actionViews.AddRange(_actionViewsCache[_swipedPosition]);
            }

            _actionViewsCache.Clear();
            _swipeThreshold = DefaultSwipeThreshold * CalculateActionViewsWidth(_actionViews);
            RecoverSwipedItem();
        }

        public override float GetSwipeThreshold(RecyclerView.ViewHolder viewHolder) => _swipeThreshold;

        public override float GetSwipeEscapeVelocity(float defaultValue) => 0.1f * defaultValue;

        public override float GetSwipeVelocityThreshold(float defaultValue) => 5.0f * defaultValue;

        public override void OnChildDraw(
            Canvas c,
            RecyclerView recyclerView,
            RecyclerView.ViewHolder viewHolder,
            float dX, float dY,
            int actionState,
            bool isCurrentlyActive)
        {
            var position = viewHolder.AdapterPosition;
            var itemView = viewHolder.ItemView;

            if (position < 0)
            {
                _swipedPosition = position;
                return;
            }

            if (actionState == ItemTouchHelper.ActionStateSwipe)
            {
                if (dX < 0) // for swipe to left
                {
                    var actionViewsBuffer = new List<ISwipeActionView>();

                    if (!_actionViewsCache.ContainsKey(position))
                    {
                        _createSwipeActionViews(viewHolder, position, actionViewsBuffer);
                        _actionViewsCache.Add(position, actionViewsBuffer);
                    }
                    else
                    {
                        actionViewsBuffer = _actionViewsCache[position];
                    }

                    var translationX = dX * CalculateActionViewsWidth(actionViewsBuffer) / itemView.Width;

                    DrawActionViews(c, itemView, actionViewsBuffer, position, translationX);

                    dX = translationX;
                }
            }

            base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
        }

        private float CalculateActionViewsWidth(IEnumerable<ISwipeActionView> actionViews) => actionViews.Sum(x => x.Width);

        private void RecoverSwipedItem()
        {
            while (_recoverQueue.Any())
            {
                var position = _recoverQueue.Dequeue();
                if (position > -1)
                {
                    _recyclerViewRef.Target.GetAdapter().NotifyItemChanged(position);
                }
            }
        }

        /// <summary>
        /// Draws the action views.
        /// </summary>
        /// <param name="c">Canvas for draw.</param>
        /// <param name="itemView">itemView of ViewHolder.</param>
        /// <param name="swipeActionViews">Collection of swipe actions views.</param>
        /// <param name="pos">ViewHolder position.</param>
        /// <param name="dX">Swipe offset.</param>
        private void DrawActionViews(Canvas c, View itemView, ICollection<ISwipeActionView> swipeActionViews, int pos, float dX)
        {
            float right = itemView.Right;
            float width = -1 * dX / swipeActionViews.Count;

            foreach (var swipeActionView in swipeActionViews)
            {
                float left = right - width;
                swipeActionView.OnDraw(c, new RectF(left, itemView.Top, right, itemView.Bottom), pos);
                right = left;
            }
        }

        private void RecyclerViewTouchHandler(object sender, View.TouchEventArgs e)
        {
            e.Handled = false;

            var motionEvent = e.Event;

            if (_swipedPosition < 0)
            {
                return;
            }

            var swipedViewHolder = _recyclerViewRef.Target.FindViewHolderForAdapterPosition(_swipedPosition);

            if (swipedViewHolder == null)
            {
                return;
            }

            var swipedItem = swipedViewHolder.ItemView;
            var rect = new Rect();
            swipedItem.GetGlobalVisibleRect(rect);

            if (motionEvent.Action == MotionEventActions.Down ||
                motionEvent.Action == MotionEventActions.Up ||
                motionEvent.Action == MotionEventActions.Move)
            {
                var point = new Point((int) motionEvent.RawX, (int) motionEvent.RawY);

                if (rect.Top < point.Y && rect.Bottom > point.Y)
                {
                    _gestureDetector.OnTouchEvent(motionEvent);
                }
                else
                {
                    _recoverQueue.Enqueue(_swipedPosition);
                    _swipedPosition = -1;
                    RecoverSwipedItem();
                }
            }
        }

        private void OnSingleTapConfirmedHandler(MotionEvent e)
        {
            foreach (var button in _actionViews)
            {
                if (button.OnClick(e.GetX(), e.GetY()))
                {
                    // collapse buttons
                    if (_recyclerViewRef.Target != null && _swipedPosition >= 0)
                    {
                        _recyclerViewRef.Target.GetAdapter().NotifyItemChanged(_swipedPosition);
                    }

                    break;
                }
            }
        }

        private class CustomGestureListener : GestureDetector.SimpleOnGestureListener
        {
            private readonly WeakAction<MotionEvent> _singleTapConfirmedHandler;

            public CustomGestureListener(Action<MotionEvent> singleTapConfirmedHandler)
            {
                _singleTapConfirmedHandler = new WeakAction<MotionEvent>(singleTapConfirmedHandler);
            }

            public override bool OnSingleTapConfirmed(MotionEvent e)
            {
                if (_singleTapConfirmedHandler.IsAlive)
                {
                    _singleTapConfirmedHandler.Execute(e);
                }
                return true;
            }
        }

        public interface ISwipeActionView
        {
            float Width { get; }
            bool OnClick(float x, float y);
            void OnDraw(Canvas canvas, RectF rect, int position);
        }
    }

    public class SimpleSwipeActionView : SwipeCallback.ISwipeActionView
    {
        protected readonly String _text;
        protected readonly Options _options;
        private readonly Action<int> _clickHandler;

        private int _position;
        private RectF _clickRegion;

        public SimpleSwipeActionView(string text, Options options, Action<int> clickHandler)
        {
            _text = text;
            _options = options;
            _clickHandler = clickHandler;
        }

        public float Width => _options.Width;

        public bool OnClick(float x, float y)
        {
            if (_clickRegion == null || !_clickRegion.Contains(x, y))
            {
                return false;
            }

            _clickHandler.Invoke(_position);
            return true;
        }

        public void OnDraw(Canvas canvas, RectF rect, int position)
        {
            DrawOnCanvas(canvas, rect);

            _clickRegion = rect;
            _position = position;
        }

        protected virtual void DrawOnCanvas(Canvas canvas, RectF rect)
        {
            var rectHeight = rect.Height();
            var rectWidth = rect.Width();
            var paint = new Paint();
            var scale = Math.Min(1, rectWidth / Width);

            // draw background
            paint.Color = _options.BackgroundColor;
            canvas.DrawRect(rect, paint);

            // draw Text
            paint.Color = Color.Argb((int) (255 * scale), 255, 255, 255);
            paint.TextSize = _options.TextSize * scale;
            paint.TextAlign = Paint.Align.Left;

            var textRect = new Rect();
            paint.GetTextBounds(_text, 0, _text.Length, textRect);
            float x = rectWidth / 2f - textRect.Width() / 2f - textRect.Left;
            float y = rectHeight / 2f + textRect.Height() / 2f - textRect.Bottom;
            canvas.DrawText(_text, rect.Left + x, rect.Top + y, paint);
        }

        public class Options
        {
            public float Width { get; set; }
            public float TextSize { get; set; }
            public Color BackgroundColor { get; set; }
        }
    }
}
