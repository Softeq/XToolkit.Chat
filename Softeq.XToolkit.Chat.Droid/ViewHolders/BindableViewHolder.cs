// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using Android.Views;
using Android.Support.V7.Widget;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.Extensions;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public abstract class BindableViewHolder<T> : RecyclerView.ViewHolder, IBindableViewHolder<T>
    {
        protected List<Binding> Bindings { get; } = new List<Binding>();

        protected BindableViewHolder(View itemView) : base(itemView) { }

        public abstract void BindViewModel(T viewModel);

        public void UnbindViewModel()
        {
            Bindings.DetachAllAndClear();
        }

        public void BindViewModel(object viewModel)
        {
            throw new System.NotImplementedException();
        }
    }
}
