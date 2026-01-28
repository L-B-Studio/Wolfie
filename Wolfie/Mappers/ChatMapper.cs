using System;
using System.Collections.Generic;
using System.Text;
using Wolfie.Models;
using Wolfie.Models.DTOObjects.Chats;

namespace Wolfie.Mappers
{
    public static class ChatMapper
    {
        public static ChatItemModel ToChatModel(GetChatResponse dto)
        {
            if (dto == null) return default ;

            return new ChatItemModel
            {
                ChatId = dto.chat_uid,
                ChatTitle = dto.chat_name ?? "Без назви",
                LastMessage = dto.last_message_text ?? "Повідомлень немає",
                Sender = dto.last_message_sender,
                // Виправив можливу помилку в назві властивості DTO
                UnreadedMessageCount = dto.not_read_messages_countun
            };
        }
    }
}
