using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageClient.Models
{
    /// <summary>
    /// Входящее сообщение от сервера
    /// </summary>
    public class RecieveMessage
    {
        public string UsernameFrom {  get; set; }
        public DateTime DateTime {  get; set; }
        public string Content { get; set; }
    }
}
