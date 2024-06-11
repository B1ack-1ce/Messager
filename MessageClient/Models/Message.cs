using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageClient.Models
{
    public class Message
    {
        public string Text { get; set; }
        public DateTime DateTime { get; set; }
        public string NicknameFrom { get; set; }
        public string NicknameTo { get; set; }

    }
}
