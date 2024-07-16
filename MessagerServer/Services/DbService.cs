using MessagerServer.Data;
using Messenger.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagerServer.Services
{
    public class DbService : IDbService
    {
        private readonly ClientsContext _context;
        private object _locker = new object();

        public DbService()
        {
            _context = new ClientsContext();
        }

        public async Task<ClientDto>? FindRegisteredUser(string email)
        {
            var user = await _context.Clients.FirstOrDefaultAsync(x => x.Email == email);

            if (user is not null)
            {
                var userDto = new ClientDto
                {
                    Username = user.Name,
                    Email = email,
                    Password = user.Password
                };

                return userDto;
            }
            return null;
        }

        public async Task<ClientDto> AddNewUser(Client newClient)
        {
            await _context.Clients.AddAsync(newClient);
            var id = _context.SaveChanges();

            var userDto = new ClientDto
            {
                Email = newClient.Email,
                Username = newClient.Name,
                Password = newClient.Password
            };

            return userDto;
        }
    }
}
