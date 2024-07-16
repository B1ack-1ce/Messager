using MessagerServer.Logging;
using Microsoft.EntityFrameworkCore;

namespace Messenger.Data
{
    public class ClientsContext : DbContext
    {
        public ClientsContext() 
        { 

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .LogTo(LogToFile.WriteToFile, Microsoft.Extensions.Logging.LogLevel.Debug)
                .UseSqlite(@"Data Source=E:\Учеба Digital-master\Messager\MessagerServer\clients.db");
        }
        public DbSet<Client> Clients => Set<Client>();
    }
}
