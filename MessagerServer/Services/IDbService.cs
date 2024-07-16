using MessagerServer.Data;
using Messenger.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagerServer.Services
{
    public interface IDbService
    {
        public Task<ClientDto>? FindRegisteredUser(string email);
        public Task<ClientDto> AddNewUser(Client newClient);
    }
}
