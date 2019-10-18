// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CoreGraphics;
using Foundation;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Common.Commands;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    [Register(nameof(CheckBoxView))]
    public class CheckBoxView : UIButton, INotifyPropertyChanged
    {
        protected CheckBoxView(IntPtr handle) : base(handle)
        {
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
            Initialize();
#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor
        }

        protected CheckBoxView(CGRect frame) : base(frame)
        {
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor
            Initialize();
#pragma warning restore RECS0021 // Warns about calls to virtual member functions occuring in the constructor
        }

        protected virtual void Initialize()
        {
            SetBackgroundImage(UIImage.FromBundle(StyleHelper.Style.CheckedBoundleName), UIControlState.Selected);
            SetBackgroundImage(UIImage.FromBundle(StyleHelper.Style.UnCheckedBoundleName), UIControlState.Normal);

            this.SetCommand(new RelayCommand(() =>
            {
                Selected = !Selected;

                RaisePropertyChanged(nameof(Selected));
            }));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
