﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Windows.Input;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.WhiteLabel.Interfaces;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatUserViewModel : ObservableObject, IViewModelParameter<ChatUserModel>
    {
        private ChatUserModel _model;
        private bool _isSelected;
        private bool _isSelectable;
        private ICommand _selectionCommand;

        public ChatUserModel Parameter
        {
            get => _model;
            set => _model = value;
        }

        public string Id => _model.Id;
        public string Username => _model.Username;
        public string PhotoUrl => _model.PhotoUrl;
        public bool IsOnline => _model.IsOnline;
        public bool IsActive => _model.IsActive;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (Set(ref _isSelected, value))
                {
                    _selectionCommand?.Execute(this);
                }
            }
        }

        public bool IsSelectable
        {
            get => _isSelectable;
            set => Set(ref _isSelectable, value);
        }

        public bool IsRemovable { get; set; }

        public void SetSelectionCommand(ICommand selectionCommand)
        {
            _selectionCommand = selectionCommand;
        }
    }
}
