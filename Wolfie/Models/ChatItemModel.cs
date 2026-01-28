using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Wolfie.Models
{
    public partial class ChatItemModel : ObservableObject
    {
        public string ChatId { get; set; } = string.Empty;
        public string ChatTitle { get; set; } = string.Empty;
        public string ChatType { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;

        [ObservableProperty]
        private string lastMessage = string.Empty;

        [ObservableProperty]
        private int? unreadedMessageCount = 0;

        // Вспомогательное свойство для инициала (Аватара)
        public string Initial => !string.IsNullOrWhiteSpace(ChatTitle) ? ChatTitle[0].ToString().ToUpper() : "?";
    }
}
