using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Wolfie.Models
{
    public class JsonPackage
    {
        public string header { get; set; }
        public Dictionary<string , string> body { get; set; }
    }
}
