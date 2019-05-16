using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatDetailsHeaderViewModel : ObservableObject
    {
        private readonly IUploadImageService _uploadImageService;
        private readonly IChatsListManager _chatsListManager;
        private readonly ChatSummaryModel _chatModel;

        private bool _isBusy;
        private bool _isMuted;
        private bool _isInEditMode;
        private string _chatName;
        private string _avatarUrl;

        public ChatDetailsHeaderViewModel(
            ChatSummaryModel chatModel,
            IUploadImageService uploadImageService,
            IChatsListManager chatsListManager)
        {
            _chatModel = chatModel;
            _uploadImageService = uploadImageService;
            _chatsListManager = chatsListManager;

            ChatName = chatModel.Name;
            AvatarUrl = chatModel.PhotoUrl;
            IsMuted = chatModel.IsMuted;

            StartEditingCommand = new RelayCommand(StartEditing);
            ChangeMuteNotificationsCommand = new RelayCommand(() => ChangeMuteNotificationsAsync().SafeTaskWrapper());
            SaveCommand = new RelayCommand<Func<(Task<Stream>, string)>>(x => SaveAsync(x).SafeTaskWrapper());
        }

        public string ChatName
        {
            get => _chatName;
            set => Set(ref _chatName, value);
        }

        public string AvatarUrl
        {
            get => _avatarUrl;
            set => Set(ref _avatarUrl, value);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set => Set(ref _isMuted, value);
        }

        public bool IsInEditMode
        {
            get => _isInEditMode;
            private set => Set(ref _isInEditMode, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value);
        }

        public ICommand StartEditingCommand { get; }

        public ICommand ChangeMuteNotificationsCommand { get; }

        public ICommand SaveCommand { get; }

        private async Task ChangeMuteNotificationsAsync()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            if (IsMuted)
            {
                await _chatsListManager.UnMuteChatAsync(_chatModel.Id).ConfigureAwait(false);
            }
            else
            {
                await _chatsListManager.MuteChatAsync(_chatModel.Id).ConfigureAwait(false);
            }

            Execute.OnUIThread(() =>
            {
                IsMuted = _chatModel.IsMuted = !IsMuted;
                IsBusy = false;
            });
        }

        private void StartEditing()
        {
            if (IsInEditMode)
            {
                return;
            }

            IsInEditMode = true;
        }

        private async Task SaveAsync(Func<(Task<Stream> GetImageTask, string Extension)> getImageFunc)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            IsInEditMode = false;

            var imagePath = await _uploadImageService.UploadImageAsync(getImageFunc);

            if (!string.IsNullOrEmpty(imagePath))
            {
                AvatarUrl = imagePath;
                _chatModel.PhotoUrl = imagePath;
            }

            if (!string.IsNullOrWhiteSpace(_chatName))
            {
                _chatModel.Name = _chatName.Trim();
            }

            await _chatsListManager.EditChatAsync(_chatModel).ConfigureAwait(false);

            Execute.BeginOnUIThread(() =>
            {
                IsBusy = false;
                ChatName = _chatModel.Name;
            });
        }
    }
}
