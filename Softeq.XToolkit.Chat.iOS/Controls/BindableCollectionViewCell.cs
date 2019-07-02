// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using System;
using System.Collections.Generic;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.Extensions;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Controls
{
    public abstract class BindableCollectionViewCell<TViewModel>
        : UICollectionViewCell, IBindableViewCell<TViewModel> where TViewModel : ObservableObject
    {
        protected readonly List<Binding> Bindings = new List<Binding>();

        protected WeakReferenceEx<TViewModel> ViewModel;

        protected BindableCollectionViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            Initialize();
        }

        public void Bind(TViewModel viewModel)
        {
            ViewModel = WeakReferenceEx.Create(viewModel);

            Bindings.DetachAllAndClear();

            DoAttachBindings();
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void DoAttachBindings()
        {
        }
    }
}
