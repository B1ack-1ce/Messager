using Microsoft.EntityFrameworkCore;

namespace Messenger.Data
{
    public class Client
    {
        public int Id { get; set; }
        public Guid ClientGuid { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
    }
}
