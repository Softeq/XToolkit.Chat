// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatMessageViewModel : ObservableObject, IViewModelParameter<ChatMessageModel>, IEquatable<ChatMessageViewModel>
    {
        private readonly IFormatService _formatService;

        public ChatMessageViewModel(IFormatService formatService)
        {
            _formatService = formatService;
        }

        //TODO Yauhen Sampir: Remove Parameter and navigate with parameter exactly on Model with execution Method UpdateMessageModel() if needed
        public ChatMessageModel Parameter
        {
            set => UpdateMessageModel(value);
            get => null;
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
        public string AttachmentImageUrl => Model?.ImageUrl;
        public bool HasAttachment => !string.IsNullOrEmpty(AttachmentImageUrl?.Trim());

        // TODO: add implementation of imageViewer
        public void OpenImage()
        {
            //_fullScreenPhotosService.DisplayImages(new List<string> { AttachmentImageUrl }, 0);
        }

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

        private void UpdateMessageModel(ChatMessageModel chatMessageModel)
        {
            Model = chatMessageModel;
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
