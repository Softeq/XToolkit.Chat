// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.Extensions;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Weak;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Controls
{
    public abstract class BindableTableViewCell<TViewModel>
       : UITableViewCell, IBindableViewCell<TViewModel> where TViewModel : ObservableObject
    {
        protected readonly List<Binding> Bindings = new List<Binding>();

        protected WeakReferenceEx<TViewModel> ViewModel;

        protected BindableTableViewCell(IntPtr handle) : base(handle)
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
