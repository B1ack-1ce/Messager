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
        private  StreamReader _reader;
        private  StreamWriter _writer;
        
        private object _locker = new object();
        private readonly UserInformation _userInformation;
        private int _countOfConnections = 0;

        public MessagerClient()
        {
            _client = new TcpClient();
            _userInformation = new UserInformation();
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

                    var sendTask = SendingMessageAsync();
                    var recieveTask = RecieveMessageAsync();

                    await Console.Out.WriteLineAsync("Ожидаем завершение одной из задач...");

                    Task.WaitAll(sendTask, recieveTask);

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

                        _reader = new StreamReader(_client.GetStream());
                        _writer = new StreamWriter(_client.GetStream());
                        _writer.AutoFlush = true;

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
            await Task.Delay(10);
            while (true)
            {
                var content = Console.ReadLine();
                
                if (content is not null && !content.Equals(string.Empty))
                {
                    var message = new Message
                    {
                        Username = _userInformation.UserName,
                        DateTime = DateTime.Now,
                        Content = content
                    };

                    var contentString = JsonConvert.SerializeObject(message);
                    await _writer.WriteLineAsync(contentString);
                }
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
                var content = await _reader.ReadLineAsync();
                if (content is not null)
                {
                    var message = JsonConvert.DeserializeObject<Message>(content);

                    if (message is not null && message.Content is not null)
                    {
                        lock (_locker)
                        {
                            Console.WriteLine($"\nСообщение пришло от: {message.Username}");
                            Console.WriteLine($"Дата {message.DateTime.ToShortTimeString()}");
                            Console.WriteLine($"Контент: {message.Content}");
                        }
                    }
                    else
                    {
                        var userInfo = JsonConvert.DeserializeObject<UserInformation>(content);

                        if (userInfo is not null)
                        {
                            _userInformation.UserName = userInfo.UserName;
                            _userInformation.Email = userInfo.Email;
                        }
                    }
                }
            }
        }
    }
}
