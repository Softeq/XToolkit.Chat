// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.ComponentModel;
using Softeq.XToolkit.Chat.Exceptions;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ConnectionStatusViewModel : ObservableObject, IDisposable
    {
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly IChatConnectionManager _chatConnectionManager;
        
        private IDisposable _connectionStatusChangedSubscription;
        private string _onlineTextStatus;
        private string _connectionStatusText;
        private bool _isOnline;

        public ConnectionStatusViewModel(
            IChatLocalizedStrings localizedStrings,
            IChatConnectionManager chatConnectionManager)
        {
            _localizedStrings = localizedStrings;
            _chatConnectionManager = chatConnectionManager;
        }

        public string ConnectionStatusText
        {
            get => _connectionStatusText;
            private set => Set(ref _connectionStatusText, value);
        }

        public bool IsOnline
        {
            get => _isOnline;
            private set => Set(ref _isOnline, value);
        }

        public void Initialize(string onlineTextStatus)
        {
            _onlineTextStatus = onlineTextStatus ?? throw new ArgumentNullException(nameof(onlineTextStatus));
            _connectionStatusChangedSubscription = _chatConnectionManager.ConnectionStatusChanged.Subscribe(UpdateConnectionStatus);

            UpdateConnectionStatus(_chatConnectionManager.ConnectionStatus);
        }
        
        private void UpdateConnectionStatus(ConnectionStatus status)
        {
            if (_onlineTextStatus == null)
            {
                throw new ChatException("Need to call Initialize() method before.");
            }
            
            switch (status)
            {
                case ConnectionStatus.Online:
                    ConnectionStatusText = _onlineTextStatus;
                    break;
                case ConnectionStatus.WaitingForNetwork:
                    ConnectionStatusText = _localizedStrings.WaitingForNetwork;
                    break;
                case ConnectionStatus.Updating:
                    ConnectionStatusText = _localizedStrings.Updating;
                    break;
                case ConnectionStatus.Connecting:
                    ConnectionStatusText = _localizedStrings.Connecting;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            IsOnline = status == ConnectionStatus.Online;
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connectionStatusChangedSubscription?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
