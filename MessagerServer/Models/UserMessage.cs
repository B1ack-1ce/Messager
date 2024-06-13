using Newtonsoft.Json;

namespace MessagerServer.Models
{
    /// <summary>
    /// Входящее сообщение от клиента к серверу
    /// </summary>
    public class UserMessage
    {
        public Guid ClientId { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
        public string Username { get; set; }
    }
}
