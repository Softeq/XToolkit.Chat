// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.Chat.Models.Interfaces;
using System.Linq;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.WhiteLabel.ImagePicker;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatMessagesViewModel : ViewModelBase, IViewModelParameter<ChatSummaryViewModel>
    {
        private readonly ChatManager _chatManager;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly IFormatService _formatService;
        private readonly IImagePickerService _imagePicker;

        private ChatSummaryViewModel _chatSummaryViewModel;

        public ChatMessagesViewModel(
            IImagePickerService imagePicker,
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService,
            ChatManager chatManager,
            ConnectionStatusViewModel connectionStatusViewModel)
        {
            _imagePicker = imagePicker;
            _pageNavigationService = pageNavigationService;
            _localizedStrings = localizedStrings;
            _formatService = formatService;
            _chatManager = chatManager;

            ConnectionStatus = connectionStatusViewModel;

            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            ShowInfoCommand = new RelayCommand(ShowInfo);
        }

        public ChatSummaryViewModel Parameter
        {
            get => _chatSummaryViewModel;
            set
            {
                _chatSummaryViewModel = value;

                CreateSubViewModels(_chatSummaryViewModel.ChatId);

                RaisePropertyChanged(nameof(ChatName));
            }
        }

        public ConnectionStatusViewModel ConnectionStatus { get; }
        public ChatMessagesListViewModel MessagesList { get; private set; }
        public ChatMessageInputViewModel MessageInput { get; private set; }

        public string ChatName => _chatSummaryViewModel?.ChatName;
        public bool HasInfo => !_chatSummaryViewModel.IsDirect;

        public ICommand BackCommand { get; }
        public ICommand ShowInfoCommand { get; }
        public ICommand MessageAddedCommand { get; set; } // TODO YP: review this approach

        public override void OnAppearing()
        {
            base.OnAppearing();

            _chatManager.MakeChatActive(_chatSummaryViewModel.ChatId);

            ConnectionStatus.Initialize(ChatName);
            MessagesList.OnAppearing();

            // fast reset unread indicator
            _chatSummaryViewModel.UnreadMessageCount = 0;
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();

            ConnectionStatus.RemoveSubscriptions();
            MessagesList.OnDisappearing();
        }

        public virtual string GetDateString(DateTimeOffset date)
        {
            return _formatService.Humanize(date, _localizedStrings.Today, _localizedStrings.Yesterday);
        }

        public IReadOnlyList<CommandAction> GetCommandActionsForMessage(ChatMessageViewModel message)
        {
            var actionCommands = new List<CommandAction>();

            if (CanEditMessage(message))
            {
                actionCommands.Add(new CommandAction
                {
                    Title = _localizedStrings.Edit,
                    Command = new RelayCommand<ChatMessageViewModel>(EditMessage),
                    CommandActionStyle = CommandActionStyle.Default
                });
            }

            actionCommands.Add(new CommandAction
            {
                Title = _localizedStrings.Delete,
                Command = new RelayCommand<ChatMessageViewModel>(DeleteMessage),
                CommandActionStyle = CommandActionStyle.Destructive
            });

            return actionCommands;
        }

        private void ShowInfo()
        {
            _pageNavigationService.For<ChatDetailsViewModel>()
                .WithParam(x => x.Summary, _chatSummaryViewModel.Parameter)
                .Navigate();
        }

        private void CreateSubViewModels(string chatId)
        {
            MessagesList = new ChatMessagesListViewModel(chatId, _chatManager, () =>
            {
                MessageAddedCommand?.Execute(null);
            });

            MessageInput = new ChatMessageInputViewModel(chatId, _chatManager, _imagePicker, _localizedStrings);
        }

        private async void DeleteMessage(ChatMessageViewModel message)
        {
            if (message == null)
            {
                return;
            }
            await _chatManager.DeleteMessageAsync(_chatSummaryViewModel.ChatId, message.Id).ConfigureAwait(false);
        }

        private void EditMessage(ChatMessageViewModel message)
        {
            MessageInput.EditMessageCommand.Execute(message);
        }

        private bool CanEditMessage(ChatMessageViewModel message)
        {
            var messages = MessagesList.Messages.Values.Where(x => x.MessageType == MessageType.Default).ToList();
            var messagePosition = messages.IndexOf(message);
            var lastMessagesBelow = messages.Skip(messagePosition + 1);
            var flowOfMineMessages = lastMessagesBelow.All(x => x.IsMine);
            return flowOfMineMessages;
        }
    }
}
