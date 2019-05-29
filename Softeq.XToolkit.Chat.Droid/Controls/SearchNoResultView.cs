using System;
using Android.Content;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Util;
using Android.Widget;

namespace Softeq.XToolkit.Chat.Droid.Controls
{
    [Register("com.softeq.xtoolkit.chat.droid.SearchNoResultView")]
    public class SearchNoResultView : ConstraintLayout
    {
        private TextView _textView;

        public SearchNoResultView(Context context) : base(context)
        {
            Initialize(context);
        }

        public SearchNoResultView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context);
        }

        public SearchNoResultView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize(context);
        }

        public string Text
        {
            get => _textView.Text;
            set => _textView.Text = value;
        }

        private void Initialize(Context context)
        {
            Inflate(context, Resource.Layout.view_search_no_results, this);
            _textView = FindViewById<TextView>(Resource.Id.search_no_result_label);
        }
    }
}
