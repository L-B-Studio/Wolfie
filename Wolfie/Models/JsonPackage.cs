using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Wolfie.Models
{
    public class SendJsonPackage
    {
        public string command { get; set; }
        public object data { get; set; }
    }
}
