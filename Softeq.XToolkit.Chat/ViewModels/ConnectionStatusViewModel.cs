// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.ComponentModel;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ConnectionStatusViewModel : ObservableObject
    {
        private readonly IChatLocalizedStrings _localizedStrings;

        public ConnectionStatusViewModel(IChatLocalizedStrings localizedStrings)
        {
            _localizedStrings = localizedStrings;
        }

        public string ConnectionStatusText { get; private set; }

        public bool IsOnline { get; private set; }

        public void UpdateConnectionStatus(ConnectionStatus status, string onlineStatusText)
        {
            switch (status)
            {
                case ConnectionStatus.Online:
                    ConnectionStatusText = onlineStatusText;
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

            RaisePropertyChanged(nameof(ConnectionStatusText));
            RaisePropertyChanged(nameof(IsOnline));
        }
    }
}
