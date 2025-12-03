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
        public event PropertyChangedEventHandler PropertyChanged;


        private readonly ChatStorageService _storage;


        public ObservableCollection<MessageItem> Messages { get; private set; }


        private string _inputText;
        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(); }
        }


        public ChatItem Chat { get; private set; }

        public MessageItem Message { get; private set; }

        public Command SendCommand { get; }


        public ChatPageViewModel(ChatItem chat,MessageItem message , ChatStorageService storage)
        {
            Chat = chat;
            Message = message;
            _storage = storage;
            Messages = _storage.GetMessages(chat.ChatId);


            SendCommand = new Command(() =>
            {
                if (string.IsNullOrWhiteSpace(InputText)) return;


                _storage.AddMessage(chat.ChatId,message.MessageId , message.Sender , message.Getter ,  InputText);
                InputText = string.Empty;
            });
        }


        void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
