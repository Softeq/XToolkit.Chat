// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.Droid.ViewHolders
{
    public interface IBindableViewHolder<T> : IBindableViewHolder
    {
        void BindViewModel(T viewModel);
    }

    public interface IBindableViewHolder : IDisposable
    {
        void BindViewModel(object viewModel);
        void UnbindViewModel();
    }
}
