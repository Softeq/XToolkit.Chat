// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.Chat.Strategies.Search;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Model;
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
        private bool _isInEditMode;
        private string _chatName;
        private string _avatarUrl;

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
            LocalizedStrings = localizedStrings;
            _formatService = formatService;
            _uploadImageService = uploadImageService;
            _dialogsService = dialogsService;
            _chatService = chatService;
        }

        public ChatSummaryModel Summary { get; set; }

        public IChatLocalizedStrings LocalizedStrings { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public bool IsInEditMode
        {
            get => _isInEditMode;
            set => Set(ref _isInEditMode, value);
        }

        public string ChatName
        {
            get => _chatName;
            set
            {
                if (Set(ref _chatName, value))
                {
                    Summary.Name = value;
                }
            }
        }

        public string AvatarUrl
        {
            get => _avatarUrl;
            set
            {
                if (Set(ref _avatarUrl, value))
                {
                    Summary.AvatarUrl = value;
                }
            }
        }

        public bool CanEdit => Summary.IsCreatedByMe;

        public ObservableRangeCollection<ChatUserViewModel> Members { get; }
            = new ObservableRangeCollection<ChatUserViewModel>();

        public string MembersCountText => _formatService.PluralizeWithQuantity(Members.Count,
            LocalizedStrings.MembersPlural, LocalizedStrings.MemberSingular);

        public bool IsMuted
        {
            get => Summary.IsMuted;
            set
            {
                if (Summary.IsMuted != value)
                {
                    Summary.IsMuted = value;
                    RaisePropertyChanged(nameof(IsMuted));
                }
            }
        }

        public ICommand AddMembersCommand { get; private set; }

        public ICommand BackCommand { get; private set; }

        public ICommand ChangeMuteNotificationsCommand { get; private set; }

        public ICommand RemoveMemberCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        public override void OnInitialize()
        {
            base.OnInitialize();

            ChatName = Summary.Name;
            AvatarUrl = Summary.AvatarUrl;

            AddMembersCommand = new RelayCommand(AddMembers);
            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            RemoveMemberCommand = new RelayCommand<ChatUserViewModel>(x => RemoveMemberAsync(x).SafeTaskWrapper());
            ChangeMuteNotificationsCommand = new RelayCommand(() => ChangeMuteNotificationsAsync().SafeTaskWrapper());
            SaveCommand = new RelayCommand<Func<(Task<Stream>, string)>>(x => SaveAsync(x).SafeTaskWrapper());
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

            await _chatService.DeleteMemberAsync(Summary.Id, memberViewModel.Id);

            RaisePropertyChanged(nameof(MembersCountText));
        }

        private async Task ChangeMuteNotificationsAsync()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            if (IsMuted)
            {
                await _chatService.UnMuteChatAsync(Summary.Id).ConfigureAwait(false);
            }
            else
            {
                await _chatService.MuteChatAsync(Summary.Id).ConfigureAwait(false);
            }

            Execute.OnUIThread(() =>
            {
                IsMuted = !IsMuted;
                IsBusy = false;
            });
        }

        public async Task SaveAsync(Func<(Task<Stream> GetImageTask, string Extension)> getImageFunc)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            var imagePath = await UploadChatAvatarAsync(getImageFunc);

            if (!string.IsNullOrEmpty(imagePath))
            {
                AvatarUrl = imagePath;
            }

            await _chatsListManager.EditChatAsync(Summary).ConfigureAwait(false);

            Execute.BeginOnUIThread(() =>
            {
                IsBusy = false;
                IsInEditMode = false;
            });
        }

        private async void AddMembers()
        {
            var result = await _dialogsService.ShowForViewModel<AddContactsViewModel, AddContactParameters>(
                new AddContactParameters
                {
                    SelectedContacts = Members,
                    SelectionType = SelectedContactsAction.InviteToChat,
                    SearchStrategy = new InviteSearchContactsStrategy(_chatService, Summary.Id)
                },
                new OpenDialogOptions
                {
                    ShouldShowBackgroundOverlay = false
                });

            if (result != null)
            {
                result.SelectedContacts.Apply(x => x.IsSelectable = false);
                ApplyRemovable(result.SelectedContacts);
                Members.AddRange(result.SelectedContacts);

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
                member.IsRemovable = Summary.IsCreatedByMe && member.Id != Summary.CreatorId;
            });
        }

        // TODO YP: dublicate CreateChatViewModel logic
        private async Task<string> UploadChatAvatarAsync(Func<(Task<Stream> GetImageTask, string Extension)> getImageFunc)
        {
            var (GetImageTask, Extension) = getImageFunc();
            var imagePath = default(string);

            using (var image = await GetImageTask.ConfigureAwait(false))
            {
                if (image != null)
                {
                    imagePath = await _uploadImageService.UploadImageAsync(image, Extension)
                        .ConfigureAwait(false);
                }
            }

            return imagePath;
        }
    }
}