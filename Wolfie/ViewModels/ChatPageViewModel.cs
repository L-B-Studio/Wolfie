using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Wolfie.Models;
using Wolfie.Services;

namespace Wolfie.ViewModels
{
    public class ChatPageViewModel : INotifyPropertyChanged
    {
        private readonly ChatStorageService _chatService;

        public ObservableCollection<ChatItem> Chats => _chatService.Chats;

        public ChatPageViewModel(ChatStorageService chatService)
        {
            _chatService = chatService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
