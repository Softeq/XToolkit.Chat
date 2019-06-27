// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.Views;
using Android.Widget;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public class ConversationSystemViewHolder : BindableViewHolder<ChatMessageViewModel>
    {
        private readonly TextView _titleTextView;

        public ConversationSystemViewHolder(View itemView) : base(itemView)
        {
            _titleTextView = itemView.FindViewById<TextView>(Resource.Id.item_chat_conversation_system_title);
        }

        public override void BindViewModel(ChatMessageViewModel viewModel)
        {
            _titleTextView.Text = viewModel.Body;
        }
    }
}
