// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using CoreGraphics;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ConnectionStatusView : NotifyingView
    {
        public ConnectionStatusView(IntPtr handle) : base(handle) { }
        public ConnectionStatusView(CGRect frame) : base(frame) { }

        public void Update(ConnectionStatusViewModel viewModel)
        {
            Execute.BeginOnUIThread(() =>
            {
                TitleLabel.Text = viewModel.ConnectionStatusText;
                Spinner.Hidden = viewModel.IsOnline;
            });
        }
    }
}
