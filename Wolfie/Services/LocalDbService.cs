using System;
using System.Collections.Generic;
using System.Text;
using Wolfie.LocalData;
using Wolfie.Models;

namespace Wolfie.Services
{
    public class LocalDbService
    {
        //private readonly string? _dbpath = "LocalData\\app.db";

        //public void ChatlistStartDataBase(string dbPath = "LocalData\\chatlist.db")
        //{
        //    using var db = new AppDbContext(dbPath);
        //
        //    db.Database.EnsureCreated();
        //}
        //public void MessageStartDataBase(string dbPath = "LocalData\\message.db")
        //{
        //    using var db = new AppDbContext(dbPath);
        //
        //    db.Database.EnsureCreated();
        //}

        public async void MessageAddOrUpdateInDb(string? ChatId = null,
                                                    string? MessageId = null,
                                                    string? Sender = null,
                                                    string? Getter = null,
                                                    string? Message = null,
                                                    DateTime? MessageTime = null)
        {
            string dbPath = $"LocalData\\{ChatId.Trim().ToLower()}.db";

            using var db = new AppDbContext(dbPath);

            db.Database.EnsureCreated();

            var messagechanged = db.Message.FirstOrDefault(c => c.MessageId == $"{MessageId}");

            if (messagechanged != null)
            {
                messagechanged.Message += $"{Message}";
                db.SaveChanges();
            }
            await db.Message.AddAsync(new MessageItem
            {
                MessageId = MessageId,
                Sender = Sender,
                Getter = Getter,
                Message = Message,
                MessageTime = MessageTime
            });
        }

        public async void ListAddOrUpdateInDb(string? ChatId = null,
                                                    string? ChatTitle = null,
                                                    string? LastMessage = null)
        {
            string dbPath = "LocalData\\chatlist.db";
            using var db = new AppDbContext(dbPath);

            db.Database.EnsureCreated();

            var chatlist = db.ChatList.FirstOrDefault(c => c.ChatId == $"{ChatId}");

            if (chatlist != null)
            {
                chatlist.LastMessage += $"{LastMessage}";
                db.SaveChanges();
            }
            await db.ChatList.AddAsync(new ChatItem
            {
                ChatId = ChatId,
                ChatTitle = ChatTitle,
                LastMessage = LastMessage
            });
        }

        public async void MessageDeleteInDb(string? ChatId = null, string? MessageId = null)
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

        public async void ListDeleteInDb(string? ChatId = null, string dbPath = "LocalData\\Chats\\chatlist.db")
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
