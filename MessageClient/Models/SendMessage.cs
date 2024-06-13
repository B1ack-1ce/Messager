namespace MessageClient.Models
{
    /// <summary>
    /// Исходящее сообщение от клиента к серверу
    /// </summary>
    public class SendMessage
    {
        public Guid ClientId { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
        public string Username { get; set; }

    }
}
