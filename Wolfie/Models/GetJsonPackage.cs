using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Wolfie.Models
{
    public class GetJsonPackage
    {
        public string status { get;set; }
        public JsonElement data { get; set; }
    }
}
