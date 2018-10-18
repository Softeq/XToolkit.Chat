// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.Views;
using Android.Widget;
using Softeq.XToolkit.Common;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class ConversationInfoViewHolder : BindableViewHolder<DateTimeOffset>
    {
        private readonly WeakFunc<DateTimeOffset, string> _dataConverter;

        public ConversationInfoViewHolder(View itemView, Func<DateTimeOffset, string> dataConverter)
            : base(itemView)
        {
            _dataConverter = new WeakFunc<DateTimeOffset, string>(dataConverter);

            InfoTextView = itemView.FindViewById<TextView>(Resource.Id.tv_info);
        }

        private TextView InfoTextView { get; }

        public override void BindViewModel(DateTimeOffset date)
        {
            if (_dataConverter != null && _dataConverter.IsAlive)
            {
                InfoTextView.Text = _dataConverter.Execute(date);
            }
        }
    }
}
