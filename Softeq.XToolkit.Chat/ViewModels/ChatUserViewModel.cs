// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Windows.Input;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatUserViewModel : ViewModelBase, IViewModelParameter<ChatUserModel>
    {
        private ChatUserModel _model;
        private bool _isSelected;
        private bool _isSelectable;
        private ICommand _selectionCommand;

        public ChatUserModel Parameter
        {
            set => _model = value;
        }

        public string Id => _model.SaasUserId;
        public string Username => _model.Username;
        public string PhotoUrl => _model.PhotoUrl;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (Set(ref _isSelected, value))
                {
                    _selectionCommand?.Execute(null);
                }
            }
        }

        public bool IsSelectable
        {
            get => _isSelectable;
            set => Set(ref _isSelectable, value);
        }

        public void SetSelectionCommand(ICommand selectionCommand)
        {
            _selectionCommand = selectionCommand;
        }
    }
}
