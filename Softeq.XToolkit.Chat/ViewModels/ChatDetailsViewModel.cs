// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Manager;
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
using Softeq.XToolkit.WhiteLabel.Interfaces;
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

            AddMembersCommand = new RelayCommand(AddMembers);
            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            RemoveMemberAtCommand = new RelayCommand<int>(RemoveMemberAt);
        }

        public ChatSummaryModel Summary { get; set; }
        public IChatLocalizedStrings LocalizedStrings { get; }

        public ObservableRangeCollection<ChatUserViewModel> Members { get; }
            = new ObservableRangeCollection<ChatUserViewModel>();

        public string MembersCountText => _formatService.PluralizeWithQuantity(Members.Count,
            LocalizedStrings.MembersPlural, LocalizedStrings.MemberSingular);

        public ICommand AddMembersCommand { get; }

        public ICommand BackCommand { get; }
        public RelayCommand<int> RemoveMemberAtCommand { get; }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            Members.Clear();

            var members = await _chatsListManager.GetChatMembersAsync(Summary.Id);
            Members.AddRange(members);
            RaisePropertyChanged(nameof(MembersCountText));
        }

        public bool IsMemberRemovable(int memberPosition)
        {
            if (Summary.IsCreatedByMe)
            {
                return Members[memberPosition].Id != Summary.CreatorId;
            }

            return false;
        }

        private void RemoveMemberAt(int index)
        {
            Members.RemoveAt(index);

            //TODO YP: send request to the server, wait backend impl.

            RaisePropertyChanged(nameof(MembersCountText));
        }

        public async Task SaveAsync(Func<(Task<Stream> GetImageTask, string Extension)> getImageFunc)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            var imageInfo = getImageFunc();
            var imagePath = default(string);

            using (var image = await imageInfo.GetImageTask.ConfigureAwait(false))
            {
                if (image != null)
                {
                    imagePath = await _uploadImageService.UploadImageAsync(image, imageInfo.Extension)
                        .ConfigureAwait(false);
                }
            }

            if (imagePath != null)
            {
                Summary.AvatarUrl = imagePath;
                RaisePropertyChanged(nameof(Summary.AvatarUrl));

                await _chatsListManager.EditChatAsync(Summary).ConfigureAwait(false);
            }

            Execute.BeginOnUIThread(() =>
            {
                IsBusy = false;
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
    }
}