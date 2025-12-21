using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wolfie.Models
{
    [Table("MyInfo")]
    public class UserItemModel
    {
        [PrimaryKey]
        public string Username { get; set; } = "wolfie_guest";
        public string ChatAvatarUri { get; set; } = string.Empty;
        public bool IsPrime { get; set; } = false;
        public bool IsDeveloper { get; set; } = false;
        public bool IsLogger { get; set;  } = false;
    }
}
