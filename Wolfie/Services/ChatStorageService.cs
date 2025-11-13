
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.Text;
using Wolfie.Helpers;
using Wolfie.Models;

namespace Wolfie.Services
{
    public class ChatStorageService
    {
        public ObservableCollection<ChatItem> Chats { get; } = new();
        public Dictionary<string, ObservableCollection<MessageItem>> Messages { get; } = new();

        private readonly SslClientService _client;
        public ChatStorageService()
        {
            _client = SslClientHelper.GetService<SslClientService>();
            _client.MessageReceived += OnMessageReceivedAsync;
        }

        public async void OnMessageReceivedAsync(string msg)
        {
            var _message = msg.Trim().ToLower().Split(';').ToArray();

            switch (_message[0])
            {
                case "chatitem_data":
                    //if (_message.Length < 5) return;

                    //int length = int.Parse(_message[4]);
                    //byte[] buffer = new byte[length];
                    //int totalRead = 0;
                    //
                    //while (totalRead < length)
                    //{
                    //    int read = await stream.ReadAsync(buffer, totalRead, length - totalRead);
                    //    if (read == 0) throw new Exception("Connection closed while reading avatar");
                    //    totalRead += read;
                    //}
                    //
                    //while(totalRead < length)
                    //{
                    //    
                    //}
                    //    
                    //string filePath = Path.Combine(FileSystem.CacheDirectory, $"{_message[2]}_avatar.jpg");
                    //await File.WriteAllBytesAsync(filePath, buffer);

                    Chats.Add(new ChatItem
                    {
                        ChatId = _message[1],
                        Username = _message[2],
                        LastMessage = _message[3],
                        //Avatar = filePath
                    });
                    break;

                case "messageitem_data":
                    //if (_message.Length < 5) return;

                    Messages[_message[1]].Add(new MessageItem
                    {
                        ChatId = _message[1],
                        Sender = _message[2],
                        Message = _message[3],
                        MessageTime = DateTime.Parse(_message[4])
                    });
                    break;
            }
        }
        public ObservableCollection<MessageItem> GetMessages(string chatId)
        {
            if (!Messages.ContainsKey(chatId))
                Messages[chatId] = new();


            return Messages[chatId];
        }


        public void AddMessage(string chatId, string sender, string text)
        {
            var msg = new MessageItem
            {
                ChatId = chatId,
                Sender = sender,
                Message = text,
                MessageTime = DateTime.Now
            };


            Messages[chatId].Add(msg);


            var chat = Chats.First(x => x.ChatId == chatId);
            chat.LastMessage = text;
        }
    }
}
