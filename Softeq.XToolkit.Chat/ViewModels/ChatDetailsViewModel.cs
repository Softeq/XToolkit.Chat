// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.Strategies.Search;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Commands;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatDetailsViewModel : ViewModelBase
    {
        private readonly IChatsListManager _chatsListManager;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IFormatService _formatService;
        private readonly IUploadImageService _uploadImageService;
        private readonly IDialogsService _dialogsService;
        private readonly IChatService _chatService;

        private bool _isLoading;

        public ChatDetailsViewModel(
            IChatsListManager chatsListManager,
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService,
            IUploadImageService uploadImageService,
            IDialogsService dialogsService,
            IChatService chatService)
        {
            _chatsListManager = chatsListManager;
            _pageNavigationService = pageNavigationService;
            _formatService = formatService;
            _uploadImageService = uploadImageService;
            _dialogsService = dialogsService;
            _chatService = chatService;

            LocalizedStrings = localizedStrings;
        }

        public ChatSummaryModel Summary { get; set; }

        public ChatDetailsHeaderViewModel HeaderViewModel { get; private set; }

        public IChatLocalizedStrings LocalizedStrings { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public bool CanEdit => Summary.IsCreatedByMe;

        public ObservableRangeCollection<ChatUserViewModel> Members { get; }
            = new ObservableRangeCollection<ChatUserViewModel>();

        public string MembersCountText => _formatService.PluralizeWithQuantity(Members.Count,
            LocalizedStrings.MembersPlural, LocalizedStrings.MemberSingular);

        public ICommand AddMembersCommand { get; private set; }

        public ICommand BackCommand { get; private set; }

        public ICommand RemoveMemberCommand { get; private set; }

        public override void OnInitialize()
        {
            base.OnInitialize();

            HeaderViewModel = new ChatDetailsHeaderViewModel(Summary, _uploadImageService, _chatsListManager);

            AddMembersCommand = new AsyncCommand(AddMembers);
            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            RemoveMemberCommand = new RelayCommand<ChatUserViewModel>(x => RemoveMemberAsync(x).SafeTaskWrapper());
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            LoadDataAsync().SafeTaskWrapper();
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            var members = await _chatsListManager.GetChatMembersAsync(Summary.Id).ConfigureAwait(false);
            ApplyRemovable(members);

            Execute.BeginOnUIThread(() =>
            {
                Members.ReplaceRange(members.EmptyIfNull());
                RaisePropertyChanged(nameof(MembersCountText));
                IsLoading = false;
            });
        }

        private async Task RemoveMemberAsync(ChatUserViewModel memberViewModel)
        {
            var hasRemoveConfirmation = await _dialogsService.ShowDialogAsync(
                LocalizedStrings.RemoveUserConfirmationTitle,
                LocalizedStrings.RemoveUserConfirmationMessage,
                LocalizedStrings.Yes,
                LocalizedStrings.No);

            if (!hasRemoveConfirmation)
            {
                return;
            }

            Members.Remove(memberViewModel);
            RaisePropertyChanged(nameof(MembersCountText));

            await _chatService.DeleteMemberAsync(Summary.Id, memberViewModel.Id).ConfigureAwait(false);
        }

        private async Task AddMembers()
        {
            var parameter = new AddContactParameters
            {
                SelectedContacts = Members,
                SelectionType = SelectedContactsAction.InviteToChat,
                SearchStrategy = new InviteSearchContactsStrategy(_chatService, Summary.Id)
            };

            var selectedContacts = await _dialogsService.For<AddContactsViewModel>()
                .WithParam(x => x.Parameter, parameter)
                .Navigate<ObservableRangeCollection<ChatUserViewModel>>();

            if (selectedContacts != null && selectedContacts.Count > 0)
            {
                selectedContacts.Apply(x => x.IsSelectable = false);

                ApplyRemovable(selectedContacts);
                Members.AddRange(selectedContacts);

                RaisePropertyChanged(nameof(MembersCountText));

                var contactsForInvite = Members.Where(x => x.IsSelected).Select(x => x.Id).ToList();

                try
                {
                    await _chatsListManager.InviteMembersAsync(Summary.Id, contactsForInvite);
                }
                catch (Exception ex)
                {
                    LogManager.LogError<ChatDetailsViewModel>(ex);
                }
            }
        }

        private void ApplyRemovable(IList<ChatUserViewModel> members)
        {
            members.Apply(member =>
            {
                member.IsRemovable = CanEdit && member.Id != Summary.Member.Id;
            });
        }
    }
}