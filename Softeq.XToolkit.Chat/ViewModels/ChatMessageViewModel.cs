// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.WhiteLabel.ViewModels;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatMessageViewModel : ObservableObject, IEquatable<ChatMessageViewModel>
    {
        private readonly IFormatService _formatService;
        private readonly IDialogsService _dialogsService;

        public ChatMessageViewModel(
            IFormatService formatService,
            IDialogsService dialogsService)
        {
            _formatService = formatService;
            _dialogsService = dialogsService;
        }

        public ChatMessageModel Model { get; private set; }

        public string Id => Model.Id;
        public string ChatId => Model.ChannelId;
        public DateTimeOffset DateTime => Model.DateTime;
        public string TextDateTime => _formatService.ToShortTimeFormat(Model.DateTime.LocalDateTime);

        public string Body
        {
            get => Model.Body;
            set
            {
                if (Model.Body != value)
                {
                    Model.Body = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SenderPhotoUrl => Model.SenderPhotoUrl;
        public ChatMessageStatus Status => Model.Status;
        public bool IsRead => Model.IsRead;
        public string SenderName => Model.SenderName;
        public bool IsMine => Model.IsMine;
        public MessageType MessageType => Model.MessageType;

        public bool HasAttachment => !string.IsNullOrEmpty(Model.ImageRemoteUrl) || !string.IsNullOrEmpty(Model.ImageCacheKey);

        public bool IsEarlierThan(ChatMessageViewModel message) => Model.IsEarlierThan(message?.Model);
        public bool IsEarlierOrEqualsThan(ChatMessageViewModel message) => Model.IsEarlierOrEqualsThan(message.Model);
        public bool IsLaterThan(ChatMessageViewModel message) => Model.IsLaterThan(message.Model);
        public bool IsLaterOrEqualsThan(ChatMessageViewModel message) => Model.IsLaterOrEqualsThan(message.Model);

        public void UpdateMessage(ChatMessageViewModel chatMessageViewModel)
        {
            if (chatMessageViewModel == null)
            {
                return;
            }
            UpdateMessageModel(chatMessageViewModel.Model);
        }

        public bool Equals(ChatMessageViewModel other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Model.Equals(other.Model);
        }

        public override bool Equals(object obj) => Equals(obj as ChatMessageViewModel);

        public override int GetHashCode() => Model == null ? 0 : Model.GetHashCode();

        public void ShowImage(FullScreenImageOptions options)
        {
            _dialogsService.ShowForViewModel<FullScreenImageViewModel, FullScreenImageOptions>(options);
        }

        public void UpdateMessageModel(ChatMessageModel chatMessageModel)
        {
            if (Model == null)
            {
                Model = chatMessageModel;
            }
            else
            {
                Model.UpdateMessage(chatMessageModel);
            }

            Execute.BeginOnUIThread(() =>
            {
                RaisePropertyChanged(nameof(SenderName));
                RaisePropertyChanged(nameof(Body));
                RaisePropertyChanged(nameof(Status));
                RaisePropertyChanged(nameof(TextDateTime));
            });
        }
    }
}
