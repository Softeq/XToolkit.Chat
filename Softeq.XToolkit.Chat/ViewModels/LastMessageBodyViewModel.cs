using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class LastMessageBodyViewModel : ObservableObject
    {
        private readonly IFormatService _formatService;

        private ChatMessageModel _model;

        public LastMessageBodyViewModel(IFormatService formatService, ChatMessageModel model)
        {
            _formatService = formatService;
            _model = model;
        }

        public string Username => _model?.SenderName;

        public string Body => _model?.Body;

        public bool HasBody => !string.IsNullOrEmpty(Body);

        public bool HasPhoto => !string.IsNullOrEmpty(_model?.ImageRemoteUrl);

        public ChatMessageStatus Status => _model?.Status ?? ChatMessageStatus.Other;

        public string DateTime => _formatService.ToChatDateTimeFormat(_model?.DateTime.LocalDateTime);

        public void UpdateModel(ChatMessageModel newLastMessage)
        {
            _model = newLastMessage;

            Execute.BeginOnUIThread(() =>
            {
                RaisePropertyChanged(nameof(Username));
                RaisePropertyChanged(nameof(Body));
                RaisePropertyChanged(nameof(HasPhoto));
                RaisePropertyChanged(nameof(Status));
                RaisePropertyChanged(nameof(DateTime));
            });
        }
    }
}
