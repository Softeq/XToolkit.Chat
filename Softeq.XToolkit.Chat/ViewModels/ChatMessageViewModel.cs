// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatMessageViewModel : ViewModelBase, IViewModelParameter<ChatMessageModel>, IEquatable<ChatMessageViewModel>
    {
        private readonly IFullScreenPhotosService _fullScreenPhotosService;

        public ChatMessageViewModel(IFullScreenPhotosService fullScreenPhotosService)
        {
            _fullScreenPhotosService = fullScreenPhotosService;
        }

        public RelayCommand<ChatMessageViewModel> EditRequested { get; set; }
        public RelayCommand<ChatMessageViewModel> DeleteRequested { get; set; }

        public ChatMessageModel Parameter
        {
            set => UpdateMessageModel(value);
        }

        public ChatMessageModel Model { get; private set; }

        public string Id => Model.Id;
        public string ChatId => Model.ChannelId;
        public DateTimeOffset DateTime => Model.DateTime;
        public string TextDateTime => Model.DateTime.LocalDateTime.ToString("HH:mm");

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

        public void OpenImage()
        {
            _fullScreenPhotosService.DisplayImages(new List<string> { AttachmentImageUrl }, 0);
        }

        public void RequestEdit()
        {
            EditRequested?.Execute(this);
        }

        public void RequestDelete()
        {
            DeleteRequested?.Execute(this);
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
