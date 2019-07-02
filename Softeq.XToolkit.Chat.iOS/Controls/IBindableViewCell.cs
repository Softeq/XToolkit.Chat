// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using System.ComponentModel;

namespace Softeq.XToolkit.Chat.iOS.Controls
{
    public interface IBindableViewCell<TViewModel> where TViewModel : INotifyPropertyChanged
    {
        void Bind(TViewModel viewModel);
    }
}
