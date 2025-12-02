
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using Wolfie.Helpers;
using Wolfie.Models;

namespace Wolfie.Services
{
    public class ChatStorageService
    {
        public ObservableCollection<ChatItem> Chats { get; } = new();
        public Dictionary<string, ObservableCollection<MessageItem>> Messages { get; } = new();
        private LocalDbService _db = new LocalDbService();

        private readonly SslClientService _client;
        public ChatStorageService()
        {
            _client = SslClientHelper.GetService<SslClientService>();
            _client.MessageReceived += OnMessageReceivedAsync;
        }

        public async void OnMessageReceivedAsync(string msg)
        {
            var packet = JsonSerializer.Deserialize<JsonPackage>(msg);

            switch (packet.header.ToLower().Trim())
            {
                case "chatitem_data":
                    string ChatId = packet.body["ChatId"].ToLower().Trim().ToString();
                    string ChatTitle = packet.body["Username"].ToLower().Trim().ToString();
                    string LastMessage = packet.body["LastMessage"].ToLower().Trim().ToString();
                    
                    Chats.Add(new ChatItem
                    {
                        ChatId = ChatId,
                        ChatTitle = ChatTitle,
                        LastMessage = LastMessage
                    });
                    _db.ListAddOrUpdateInDb(ChatId, ChatTitle, LastMessage);
                    break;


                case "messageitem_data":
                    var chatId = packet.body["ChatId"].ToLower().Trim();

                    // Проверяем, есть ли коллекция сообщений для данного ChatId
                    if (!Messages.ContainsKey(chatId))
                    {
                        Messages[chatId] = new ObservableCollection<MessageItem>();
                    }
                    string ? MessageId = packet.body["MessageId"].ToLower().Trim().ToString();
                    string? Sender = packet.body["Sender"].ToLower().Trim().ToString();
                    string ? Getter = packet.body["Getter"].ToLower().Trim().ToString();
                    string ? Message = packet.body["Message"].ToLower().Trim().ToString();
                    DateTime? MessageTime = DateTime.Parse(packet.body["MessageTime"]);

                    // Добавляем сообщение
                    Messages[chatId].Add(new MessageItem
                    {
                        MessageId = MessageId,
                        Sender = Sender,
                        Getter = Getter,
                        Message = Message,
                        MessageTime = MessageTime
                    });
                    _db.MessageAddOrUpdateInDb(chatId, MessageId , Sender , Getter , Message  , MessageTime);

                    break;
            }
        }

        public ObservableCollection<MessageItem> GetMessages(string chatId)
        {
            if (!Messages.ContainsKey(chatId))
                Messages[chatId] = new();


            return Messages[chatId];
        }


        public void AddMessage(string chatId , string messageId, string sender, string getter , string text)
        {
            var msg = new MessageItem
            {
                MessageId = messageId,
                Getter = getter,
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
