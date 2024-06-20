using MessageClient.Models;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessageClient
{
    public class MessagerClient
    {
        private TcpClient _client;
        private int _countOfConnections = 0;

        public MessagerClient()
        {
            _client = new TcpClient();
        }

        /// <summary>
        /// Начало работы клиента
        /// </summary>
        /// <returns></returns>
        public async Task ClientStartAsync()
        {
            while (true)
            {
                try
                {
                    if (!_client.Connected)
                    {
                        await ConnectionToServerAsync();
                    }

                    var sendingTask = Task.Run(SendingMessageAsync);
                    var recieveTask = Task.Run(RecieveMessageAsync);

                    await Console.Out.WriteLineAsync("Ожидаем завершение одной из задач...");
                    sendingTask.Wait();
                    _client.Close();
                    await Console.Out.WriteLineAsync("Завершение приложения...");
                    await Task.Delay(3000);
                    return;
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync(ex.Message);
                    await Console.Out.WriteLineAsync(ex.InnerException?.Message);

                    _client.Close();

                    await ConnectionToServerAsync();
                }
            }
        }

        /// <summary>
        /// Метод для подключения и переподключения к серверу
        /// </summary>
        /// <returns></returns>
        private async Task ConnectionToServerAsync()
        {
            var remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);

            if (_countOfConnections != 0)
            {
                _client = new TcpClient();
            }

            while (true)
            {
                try
                {
                    await _client.ConnectAsync(remoteEndPoint);

                    if (_client.Connected)
                    {
                        if (_countOfConnections == 0)
                        {
                            await Console.Out.WriteLineAsync("\nУспешное подключение к серверу.");
                        }
                        else
                        {
                            await Console.Out.WriteLineAsync("\nУспешное переподключение к серверу.");
                        }
                        _countOfConnections++;
                        return;
                    }
                }
                catch
                {
                    Console.Clear();
                    await Console.Out.WriteLineAsync($"Повторное подключение к серверу.\nПопытка No: {_countOfConnections++}");
                    await Task.Delay(2000);
                }
            }
        }

        /// <summary>
        /// Отправление сообщений на сервер
        /// </summary>
        /// <returns></returns>
        private async Task SendingMessageAsync()
        {
            var sendingMessage = new SendMessage
            {
                ClientId = Guid.NewGuid(),
                DateTime = DateTime.Now
            };

            //Здесь можно прописать логику проверки никнейма или вынести все в отдельный метод
            while (true)
            {
                await Console.Out.WriteLineAsync("Введите свое имя: ");
                var nickname = Console.ReadLine();

                if (nickname != null)
                {
                    sendingMessage.Username = nickname;
                    break;
                }
            }

            try
            {
                using (var writer = new StreamWriter(_client.GetStream()))
                {
                    while (true)
                    {
                        await Console.Out.WriteLineAsync("Введите сообщение: ");
                        sendingMessage.Content = Console.ReadLine();
                        sendingMessage.DateTime = DateTime.Now;

                        if (sendingMessage.Content == string.Empty || sendingMessage.Content == null)
                        {
                            await Console.Out.WriteLineAsync("Введена пустая строка");
                            continue;
                        }
                        else if (sendingMessage.Content.ToLower().Equals("exit"))
                        {
                            await writer.WriteLineAsync("exit");
                            return;
                        }

                        var content = JsonConvert.SerializeObject(sendingMessage);

                        await writer.WriteLineAsync(content);
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка в SendingMessageAsync()", ex);
            }
        }

        /// <summary>
        /// Получение сообщений от сервера
        /// </summary>
        /// <returns></returns>
        private async Task RecieveMessageAsync()
        {
            while (true)
            {
                var stream = _client.GetStream();
                byte[] buffer = new byte[2048];
                await stream.Socket.ReceiveAsync(buffer);

                var content = Encoding.UTF8.GetString(buffer);
                var recieveMessage = JsonConvert.DeserializeObject<RecieveMessage>(content);

                await Console.Out.WriteLineAsync($"\nПолучено сообщение:");
                await Console.Out.WriteLineAsync($"От: {recieveMessage.UsernameFrom}");
                await Console.Out.WriteLineAsync($"Content: {recieveMessage.Content}");

            }
        }
    }
}
