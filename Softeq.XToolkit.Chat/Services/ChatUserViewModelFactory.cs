// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;

namespace Softeq.XToolkit.Chat.Services
{
    internal class ChatUserViewModelFactory : IViewModelFactoryService
    {
        public TViewModel ResolveViewModel<TViewModel, TParam>(TParam param) where TViewModel : ObservableObject, IViewModelParameter<TParam>
        {
            return new ChatUserViewModel { Parameter = param as ChatUserModel } as TViewModel;
        }

        public TViewModel ResolveViewModel<TViewModel>() where TViewModel : ObservableObject
        {
            return new ChatUserViewModel() as TViewModel;
        }
    }
}
