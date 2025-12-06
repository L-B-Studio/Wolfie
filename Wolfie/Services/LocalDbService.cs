using System;
using System.Collections.Generic;
using System.Text;
using Wolfie.LocalData;
using Wolfie.Models;

namespace Wolfie.Services
{
    public class LocalDbService
    {
        public async Task MessageAddOrUpdateInDb(
    string? ChatId,
    string? MessageId,
    string? Sender,
    string? Getter,
    string? Message,
    DateTime? MessageTime)
        {
            string dbPath = $"LocalData/{ChatId.ToLower().Trim()}.db";
            using var db = new AppDbContext(dbPath);

            await db.Database.EnsureCreatedAsync();

            var message = db.Message.FirstOrDefault(x => x.MessageId == MessageId);

            if (message != null)
            {
                message.Message = Message;
            }
            else
            {
                await db.Message.AddAsync(new MessageItem
                {
                    MessageId = MessageId,
                    Sender = Sender,
                    Getter = Getter,
                    Message = Message,
                    MessageTime = MessageTime
                });
            }

            await db.SaveChangesAsync();
        }


        public async Task ListAddOrUpdateInDb(
                                              string? ChatId,
                                              string? ChatTitle,
                                              string? LastMessage)
        {
            string dbPath = "LocalData\\chatlist.db";
            using var db = new AppDbContext(dbPath);

            await db.Database.EnsureCreatedAsync();

            var chat = db.ChatList.FirstOrDefault(x => x.ChatId == ChatId);

            if (chat != null)
            {
                chat.LastMessage = LastMessage;
                chat.ChatTitle = ChatTitle;
            }
            else
            {
                await db.ChatList.AddAsync(new ChatItem
                {
                    ChatId = ChatId,
                    ChatTitle = ChatTitle,
                    LastMessage = LastMessage
                });
            }

            await db.SaveChangesAsync();
        }


        public async Task MessageDeleteInDb(string? ChatId = null, string? MessageId = null)
        {
            string dbPath = $"LocalData\\Users\\{ChatId.Trim().ToLower()}.db";

            using var db = new AppDbContext(dbPath);

            var message = db.Message.FirstOrDefault(m => m.MessageId == $"{MessageId}");

            if (message != null)
            {
                db.Message.Remove(message);
                db.SaveChanges();
            }
        }

        public async Task ListDeleteInDb(string? ChatId = null, string dbPath = "LocalData\\Chats\\chatlist.db")
        {
            //string dbPath = $"LocalData\\{ChatId.Trim().ToLower()}.db";

            using var db = new AppDbContext(dbPath);

            var chatlist = db.ChatList.FirstOrDefault(m => m.ChatId == $"{ChatId}");

            if (chatlist != null)
            {
                db.ChatList.Remove(chatlist);
                db.SaveChanges();
            }
        }
    }
}
