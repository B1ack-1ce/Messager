namespace MessageClient.Models
{
    /// <summary>
    /// Исходящее сообщение от клиента к серверу
    /// </summary>
    public class Message
    {
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }

    }
}
