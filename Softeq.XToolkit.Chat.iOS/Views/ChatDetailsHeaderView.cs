﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Windows.Input;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Transformations;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.iOS.Controls;
using Softeq.XToolkit.Chat.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.Threading;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    // TODO YP: used on chat create & details pages, need to refactoring
    public partial class ChatDetailsHeaderView : NotifyingView
    {
        public ChatDetailsHeaderView(CGRect frame) : base(frame)
        {
        }

        public UITextField ChatNameField => ChatNameTextField;

        public AutoScrollLabel ChatNameTextView => ChatNameLabel;

        public bool IsNotificationsMuted
        {
            get => !MuteChatSwitch.On;
            set => MuteChatSwitch.On = !value;
        }

        public bool IsMuteNotificationsAvailable
        {
            get => MuteChatSwitch.Enabled;
            set => MuteChatSwitch.Enabled = value;
        }

        public string ChatNamePlaceholder
        {
            set => ChatNameTextField.Placeholder = value;
        }

        public string ChatMembersCount
        {
            get => MembersCountLabel.Text;
            set => MembersCountLabel.Text = value;
        }

        public bool MembersCountLabelHidden
        {
            get => MembersCountLabel.Hidden;
            set => MembersCountLabel.Hidden = value;
        }

        public void EnableEditMode(bool isEditMode)
        {
            ChatNameLabel.Hidden = isEditMode;
            ChatNameLabelHeight.Constant = isEditMode ? 0 : 24;

            ChatNameTextField.Hidden = !isEditMode;
            ChatNameTextField.Enabled = isEditMode;

            ChatNameUnderline.Hidden = !isEditMode;
        }

        public void HideMuteContainer()
        {
            MuteContainer.Hidden = true;
        }

        public void SetAddMembersCommand(ICommand command, string label)
        {
            AddMembersButton.SetTitle(label, UIControlState.Normal);
            AddMembersButton.SetCommand(command);
        }

        public void SetChangeChatPhotoCommand(ICommand command, string label)
        {
            ChangeChatPhotoButton.SetTitle(label, UIControlState.Normal);
            ChangeChatPhotoButton.SetCommand(command);
        }

        public void HideChangeChatPhoto(bool hidden)
        {
            ChangeChatPhotoButton.Hidden = hidden;
        }

        public void SetChangeMuteNotificationsCommand(ICommand command, string label)
        {
            MuteLabel.Text = label;
            MuteChatSwitch.SetCommand(command);
        }

        public void SetChatAvatar(string photoUrl, string chatName)
        {
            if (string.IsNullOrEmpty(chatName))
            {
                return;
            }

            ChatAvatarImageView.LoadImageWithTextPlaceholder(photoUrl,
               chatName, StyleHelper.Style.GroupDetailsAvatarStyles,
               x => x.Transform(new CircleTransformation()));
        }

        public void SetChangeChatName(ICommand focusCommand)
        {
            ChatNameTextField.Enabled = true;
            ChatNameTextField.SetCommand(nameof(ChatNameTextField.EditingDidBegin), focusCommand);
        }

        public void EndEditing()
        {
            ChatNameTextField.ResignFirstResponder();
        }

        public void SetEditedChatAvatar(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            Execute.OnUIThread(() =>
            {
                EditedChatAvatarImageView.Hidden = false;
                ChatAvatarImageView.Hidden = true;
            });

            var imageSize = StyleHelper.Style.GroupDetailsAvatarStyles.Size;
            ImageService.Instance
                .LoadFile(key)
                .DownSampleInDip(imageSize.Width, imageSize.Height)
                .Transform(new CircleTransformation())
                .IntoAsync(EditedChatAvatarImageView);
        }

        protected override void Initialize()
        {
            base.Initialize();

            ChatNameTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            EditedChatAvatarImageView.Hidden = true;

            ChatNameLabel.AnimationDuration = 5;

            ChangeChatPhotoButton.SetTitleColor(StyleHelper.Style.AccentColor, UIControlState.Normal);
            AddMembersButton.SetTitleColor(StyleHelper.Style.AccentColor, UIControlState.Normal);

            ChatAvatarImageView.LoadImageAsync(StyleHelper.Style.ChatGroupNoAvatarBundleName, null);

            MuteChatSwitch.OnTintColor = StyleHelper.Style.AccentColor;
        }
    }
}