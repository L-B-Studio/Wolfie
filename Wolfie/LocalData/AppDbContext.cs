using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Wolfie.Models;

namespace Wolfie.LocalData
{
    public class AppDbContext : DbContext
    {
        public DbSet<ChatItem> ChatList { get; set; }

        public DbSet<MessageItem> Message { get; set; }

        private string _dbPath;

        public AppDbContext(string dbPath)
        {
            _dbPath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={_dbPath}");
    }
}
