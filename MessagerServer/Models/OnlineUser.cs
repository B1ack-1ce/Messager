using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessagerServer.Models
{
    public class OnlineUser
    {
        public string UserName { get; set;}
        public string Email { get; set;}
        [JsonIgnore]
        public TcpClient TcpClient { get; private set;}
        [JsonIgnore]
        public StreamReader Reader { get; private set;}
        [JsonIgnore]
        public StreamWriter Writer { get; private set;}

        public OnlineUser(TcpClient tcpClient, string email, string userName)
        {
            UserName = userName;
            Email = email;
            TcpClient = tcpClient;
            Reader = new StreamReader(tcpClient.GetStream());
            Writer = new StreamWriter(tcpClient.GetStream());
            Writer.AutoFlush = true;
        }
    }
}
