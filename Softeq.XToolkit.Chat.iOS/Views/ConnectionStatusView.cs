// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.ComponentModel;
using CoreGraphics;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ConnectionStatusView : NotifyingView
    {
        private ConnectionStatus _connectionStatus;

        private string _defaultTitle;
        private string _waitingTitle = "Waiting for network";
        private string _connectingTitle = "Connecting...";
        private string _updatingTitle = "Updating...";

        public ConnectionStatusView(IntPtr handle) : base(handle) { }
        public ConnectionStatusView(CGRect frame) : base(frame) { }

        public string DefaultTitle
        {
            get => _defaultTitle;
            set
            {
                _defaultTitle = value;
                UpdateUI();
            }
        }

        public string WaitingTitle
        {
            get => _waitingTitle;
            set
            {
                _waitingTitle = value;
                UpdateUI();
            }
        }

        public string ConnectingTitle
        {
            get => _connectingTitle;
            set
            {
                _connectingTitle = value;
                UpdateUI();
            }
        }

        public string UpdatingTitle
        {
            get => _updatingTitle;
            set
            {
                _updatingTitle = value;
                UpdateUI();
            }
        }

        public ConnectionStatus ConnectionStatus
        {
            get => _connectionStatus;
            set
            {
                _connectionStatus = value;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            var _title = string.Empty;
            switch (_connectionStatus)
            {
                case ConnectionStatus.Online:
                    _title = _defaultTitle;
                    break;
                case ConnectionStatus.Connecting:
                    _title = _connectingTitle;
                    break;
                case ConnectionStatus.Updating:
                    _title = _updatingTitle;
                    break;
                case ConnectionStatus.WaitingForNetwork:
                    _title = _waitingTitle;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
            Execute.BeginOnUIThread(() =>
            {
                TitleLabel.Text = _title;
                Spinner.Hidden = _connectionStatus == ConnectionStatus.Online;
            });
        }
    }
}
