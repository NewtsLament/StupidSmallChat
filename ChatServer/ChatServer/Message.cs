using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChatServer
{
    public class Message
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public Message(string name, string content)
        {
            Name = name;
            Content = content;
        }

        public override string ToString()
        {
            return $"{Name} wrote: {Content}\n";
        }
    }
}
