using System.Windows.Input;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.EventArguments;
using Softeq.XToolkit.WhiteLabel.ImagePicker;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.Chat.Models.Interfaces;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatMessageInputViewModel : ObservableObject
    {
        private readonly string _chatId;
        private readonly ChatManager _chatManager;
        private readonly IChatLocalizedStrings _localizedStrings;

        private ChatMessageViewModel _messageBeingEdited;
        private string _messageBody = string.Empty;
        private bool _isInEditMessageMode;

        public ChatMessageInputViewModel(
            string chatId,
            ChatManager chatManager,
            IChatLocalizedStrings localizedStrings)
        {
            _chatId = chatId;
            _chatManager = chatManager;
            _localizedStrings = localizedStrings;

            SendMessageCommand = new RelayCommand<GenericEventArgs<ImagePickerArgs>>(SendMessageAsync);
            EditMessageCommand = new RelayCommand<ChatMessageViewModel>(EditMessage);
            CancelEditingCommand = new RelayCommand(CancelEditing);
        }

        public RelayCommand<GenericEventArgs<ImagePickerArgs>> SendMessageCommand { get; }
        public RelayCommand<ChatMessageViewModel> EditMessageCommand { get; }
        public ICommand CancelEditingCommand { get; }

        public string MessageBody
        {
            get => _messageBody;
            set => Set(ref _messageBody, value);
        }

        public string EditedMessageOriginalBody => _messageBeingEdited?.Body;

        public bool IsInEditMessageMode
        {
            get => _isInEditMessageMode;
            private set => Set(ref _isInEditMessageMode, value);
        }

        public string EditMessageHeaderString => _localizedStrings.EditMessage;
        public string EnterMessagePlaceholderString => _localizedStrings.YourMessage;

        private async void SendMessageAsync(GenericEventArgs<ImagePickerArgs> e)
        {
            var photoSelector = e?.Value;
            var newMessageBody = MessageBody?.Trim();

            if (photoSelector == null && string.IsNullOrEmpty(newMessageBody))
            {
                return;
            }

            MessageBody = string.Empty;

            if (IsInEditMessageMode)
            {
                IsInEditMessageMode = false;

                await _chatManager.EditMessageAsync(_messageBeingEdited.Id, newMessageBody).ConfigureAwait(false);

                CancelEditingCommand.Execute(null);
            }
            else
            {
                await _chatManager.SendMessageAsync(_chatId, newMessageBody, e?.Value).ConfigureAwait(false);
            }
        }

        private void EditMessage(ChatMessageViewModel editedMessage)
        {
            if (editedMessage == null)
            {
                return;
            }

            _messageBeingEdited = editedMessage;

            Execute.BeginOnUIThread(() =>
            {
                MessageBody = editedMessage?.Body;
                IsInEditMessageMode = true;
            });
        }

        private void CancelEditing()
        {
            _messageBeingEdited = null;

            Execute.BeginOnUIThread(() =>
            {
                MessageBody = string.Empty;
                IsInEditMessageMode = false;
            });
        }
    }
}
