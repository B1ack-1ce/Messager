using MessageClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageClient
{
    public class MessagerClient
    {
        private readonly TcpClient _client;
        public MessagerClient()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Any, 0);
            _client = new TcpClient(localEndPoint);
        }
        public async Task ClientStart()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);
            await _client.ConnectAsync(remoteEndPoint);

            var sendingTask = Task.Run(SendingMessageAsync);
            var recieveTask = Task.Run(RecieveMessageAsync);

            await Console.Out.WriteLineAsync("Ожидаем завершение одной из задач...");
            sendingTask.Wait();
            _client.Close();
            await Console.Out.WriteLineAsync("Завершение приложения...");
            await Task.Delay(3000);
        }

        private async Task SendingMessageAsync()
        {
            await Console.Out.WriteLineAsync($"SendingMessageAsync работает в потоке {Thread.CurrentThread.ManagedThreadId}");
            await Console.Out.WriteLineAsync("Введите свое имя: ");
            var nickname = Console.ReadLine();

            var sendingMessage = new Message
            {
                DateTime = DateTime.Now,
                NicknameFrom = nickname,
                NicknameTo = "?"
            };

            using var stream = _client.GetStream();

            while (true)
            {
                await Console.Out.WriteLineAsync("Введите сообщение: ");
                sendingMessage.Text = Console.ReadLine();

                if (sendingMessage.Text == string.Empty || sendingMessage.Text == null)
                {
                    await Console.Out.WriteLineAsync("Введена пустая строка");
                    continue;
                }
                else if (sendingMessage.Text.ToLower().Equals("quit"))
                {
                    break;
                }

                var content = JsonConvert.SerializeObject(sendingMessage);
                byte[] buffer = Encoding.UTF8.GetBytes(content);

                await stream.Socket.SendAsync(buffer);
                stream.Flush();
            }
        }

        private async Task RecieveMessageAsync()
        {
            await Console.Out.WriteLineAsync($"RecieveMessageAsync работает в потоке {Thread.CurrentThread.ManagedThreadId}");
            await Console.Out.WriteLineAsync("Слушаем входящие сообщения...");
            using var stream = _client.GetStream();
            byte[] buffer = new byte[2048];
            await stream.Socket.ReceiveAsync(buffer);
        }
    }
}
