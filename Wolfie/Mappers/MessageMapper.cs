using System;
using System.Collections.Generic;
using System.Text;
using Wolfie.Models;
using Wolfie.Models.DTOObjects.Chats;
using Wolfie.Models.DTOObjects.Message;

namespace Wolfie.Mappers
{
    public static class MessageMapper
    {
        public static MessageItemModel ToMessageModel(GetMessagesResponse dto)
        {
            if (dto == null) return default;

            return new MessageItemModel
            {
                ChatUid = dto.chat_uid,
                MessageId = dto.message_id,
                SenderId = dto.sender_id,
                MessageText = dto.message_text,
                CreatedAt = dto.created_at
            };
        }
    }
}
