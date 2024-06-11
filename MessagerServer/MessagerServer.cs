using MessagerServer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace MessagerServer
{
    public class MessagerServer
    {
        private readonly ConcurrentBag<TcpClient> _clients = new ConcurrentBag<TcpClient>();

        public async Task StartServerAsync()
        {
            while (true)
            {
                var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);
                using var listener = new TcpListener(localEndPoint);
                listener.Start(100);

                await Console.Out.WriteLineAsync("Waiting...");
                var socket = await listener.AcceptTcpClientAsync();

                _clients.Add(socket);

                await Console.Out.WriteLineAsync($"Подключен новый пользователь: {socket.Client.RemoteEndPoint}");
                Task.Run(() => ReceiveUserMessage(socket));
            }
        }

        private async void ReceiveUserMessage(TcpClient client)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[2048];
                    var stream = client.GetStream();
                    await stream.Socket.ReceiveAsync(buffer);

                    var content = Encoding.UTF8.GetString(buffer);
                    var message = JsonConvert.DeserializeObject<Message>(content);

                    if (message != null)
                    {
                        await Console.Out.WriteLineAsync($"Получено сообщение на сервер:");
                        await Console.Out.WriteLineAsync($"Время получения: {message.DateTime.ToShortTimeString()}");
                        await Console.Out.WriteLineAsync($"От {message.NicknameFrom}");
                        await Console.Out.WriteLineAsync($"Контент: {message.Text}");
                    }
                    else
                    {
                        throw new SocketException(0, "Клиент разорвал подключение.");
                    }
                }
            }
            catch (SocketException ex)
            {
                _clients.TryTake(out client);
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }
    }
}
