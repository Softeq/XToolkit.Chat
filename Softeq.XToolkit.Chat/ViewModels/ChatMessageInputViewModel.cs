// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿using System.Windows.Input;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.EventArguments;
using Softeq.XToolkit.WhiteLabel.ImagePicker;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.Chat.Models.Interfaces;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatMessageInputViewModel : ObservableObject, IDisposable
    {
        private readonly string _chatId;
        private readonly ChatManager _chatManager;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly IImagePickerService _imagePicker;

        private IDisposable _imageObject;
        private ImagePickerResult _imagePickerData;
        private ChatMessageViewModel _messageBeingEdited;
        private string _messageBody = string.Empty;
        private bool _isInEditMessageMode;

        public ChatMessageInputViewModel(
            string chatId,
            ChatManager chatManager,
            IImagePickerService imagePicker,
            IChatLocalizedStrings localizedStrings)
        {
            _imagePicker = imagePicker;
            _chatId = chatId;
            _chatManager = chatManager;
            _localizedStrings = localizedStrings;

            OpenCameraCommand = new AsyncCommand(OnCameraOpenAsync);
            OpenGalleryCommand = new AsyncCommand(OnGalleryOpenAsync);
            DeleteImageCommand = new RelayCommand(OnDeleteImage);
            EditMessageCommand = new RelayCommand<ChatMessageViewModel>(EditMessage);
            CancelEditingCommand = new RelayCommand(CancelEditing);
            SendMessageCommand = new AsyncCommand(SendMessageAsync);
        }

        ~ChatMessageInputViewModel()
        {
            Dispose(false);
        }

        public ICommand OpenCameraCommand { get; }
        public ICommand OpenGalleryCommand { get; }
        public ICommand DeleteImageCommand { get; }
        public ICommand CancelEditingCommand { get; }
        public ICommand SendMessageCommand { get; }
        public RelayCommand<ChatMessageViewModel> EditMessageCommand { get; }

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

        public IDisposable ImageObject
        {
            get => _imageObject;
            private set => Set(ref _imageObject, value);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _imagePickerData?.Dispose();
                ImageObject?.Dispose();
            }
        }

        private async Task SendMessageAsync()
        {
            string imageCachePath = null;

            if (_imagePickerData?.ImageObject != null)
            {
                var path = GenerateFilePath();
                using (var imageStream = await _imagePickerData.GetStream())
                {
                    using (var fileStream = File.Create(path))
                    {
                        imageStream.Seek(0, SeekOrigin.Begin);
                        imageStream.CopyTo(fileStream);
                    }
                }
                imageCachePath = path;
            }

            var photoSelector = _imagePickerData;
            var hasImage = photoSelector?.Extension != null;
            var newMessageBody = MessageBody?.Trim();

            MessageBody = string.Empty;
            SetImage(null);

            if (!hasImage && string.IsNullOrEmpty(newMessageBody))
            {
                return;
            }

            if (IsInEditMessageMode)
            {
                IsInEditMessageMode = false;

                await _chatManager.EditMessageAsync(_messageBeingEdited.Id, newMessageBody).ConfigureAwait(false);

                CancelEditingCommand.Execute(null);
            }
            else
            {
                await _chatManager.SendMessageAsync(_chatId, newMessageBody, photoSelector, imageCachePath).ConfigureAwait(false);
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

        private async Task OnCameraOpenAsync()
        {
            var result = await _imagePicker.TakePhotoAsync().ConfigureAwait(false);
            Execute.OnUIThread(() => SetImage(result));
        }

        private async Task OnGalleryOpenAsync()
        {
            var result = await _imagePicker.PickPhotoAsync().ConfigureAwait(false);
            Execute.OnUIThread(() => SetImage(result));
        }

        private void OnDeleteImage()
        {
            SetImage(null);
        }

        private void SetImage(ImagePickerResult imagePickerResult)
        {
            _imagePickerData = imagePickerResult;
            ImageObject = _imagePickerData?.ImageObject;
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

        private string GenerateFilePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Guid.NewGuid().ToString());
        }
    }
}
