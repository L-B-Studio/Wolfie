
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

                    Chats.Add(new ChatItem
                    {
                        ChatId = packet.body["ChatId"].ToLower().Trim(),
                        Username = packet.body["Username"].ToLower().Trim(),
                        LastMessage = packet.body["LastMessage"].ToLower().Trim()
                    });

                    break;


                case "messageitem_data":
                    var chatId = packet.body["ChatId"].ToLower().Trim();

                    // Проверяем, есть ли коллекция сообщений для данного ChatId
                    if (!Messages.ContainsKey(chatId))
                    {
                        Messages[chatId] = new ObservableCollection<MessageItem>();
                    }

                    // Добавляем сообщение
                    Messages[chatId].Add(new MessageItem
                    {
                        ChatId = chatId,
                        Sender = packet.body["Sender"].ToLower().Trim(),
                        Getter = packet.body["Getter"].ToLower().Trim(),
                        Message = packet.body["Message"].ToLower().Trim(),
                        MessageTime = DateTime.Parse(packet.body["MessageTime"])
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
