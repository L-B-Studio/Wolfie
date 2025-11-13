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
    public class MessangerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private readonly ChatStorageService _storage;


        public ObservableCollection<ChatItem> Chats => _storage.Chats;


        private ChatItem _selectedChat;
        public ChatItem SelectedChat
        {
            get => _selectedChat;
            set
            {
                _selectedChat = value;
                OnPropertyChanged();


                if (value != null)
                    ChatSelected?.Invoke(value);
            }
        }


        public event Action<ChatItem> ChatSelected;


        public MessangerViewModel(ChatStorageService storage)
        {
            _storage = storage;
        }


        void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
