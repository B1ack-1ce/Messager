using MessageClient.Models;
using MessagerServer.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MessagerServer
{
    public class MessagerServer
    {
        /// <summary>
        /// Список подключенных клиентов
        /// </summary>
        private readonly Dictionary<TcpClient, Guid> _clients = new Dictionary<TcpClient, Guid>();


        /// <summary>
        /// Начало работы сервера с ожиданием запроса на завершение работы сервера
        /// </summary>
        /// <returns>Возвращает задачу (Task)</returns>
        public async Task StartServerAsync(CancellationToken token)
        {
            var waitNewClient = WaitNewClient();

            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    await Console.Out.WriteLineAsync("Запрос на завершение приложения...");
                    return;
                }
                await Task.Delay(300);
            }
        }

        /// <summary>
        /// Ожидание нового подключения (клиента)
        /// </summary>
        /// <returns>Возвращает задачу</returns>
        private async Task WaitNewClient()
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);
            using var listener = new TcpListener(localEndPoint);
            listener.Start(100);
            while (true)
            {


                await Console.Out.WriteLineAsync("Waiting...");
                var socket = await listener.AcceptTcpClientAsync();

                if (socket != null)
                {
                    await Console.Out.WriteLineAsync($"Подключен новый пользователь: {socket.Client.RemoteEndPoint}");

                    Task t1 = Task.Run(() => ReceiveUserMessage(socket));
                    // Sending task
                }
            }
        }

        /// <summary>
        /// Асинхронный метод для получения сообщений от клиента
        /// </summary>
        /// <param name="client">Клиент, от которого получаем сообщения.</param>
        /// <returns>Возвращает задачу (Task)</returns>
        private async Task ReceiveUserMessage(TcpClient client)
        {
            try
            {
                string content = string.Empty;
                UserMessage receiveMessage;

                using (var stream = new StreamReader(client.GetStream()))
                {
                    while (true)
                    {
                        content = await stream.ReadLineAsync();

                        if (content != null && content.ToLower().Equals("exit"))
                        {
                            throw new SocketException(0, "Клиент разорвал подключение.");

                        }
                        else if (content != null)
                        {
                            receiveMessage = JsonConvert.DeserializeObject<UserMessage>(content);

                            // Вывод всей информации из сообщения
                            if (receiveMessage != null)
                            {
                                await Console.Out.WriteLineAsync($"\nПолучено сообщение на сервер:");
                                await Console.Out.WriteLineAsync($"Время получения: {receiveMessage.DateTime.ToShortTimeString()}");
                                await Console.Out.WriteLineAsync($"От {receiveMessage.Username}");
                                await Console.Out.WriteLineAsync($"Guid {receiveMessage.ClientId}");
                                await Console.Out.WriteLineAsync($"Контент: {receiveMessage.Content}");
                            }
                        }
                        else
                        {
                            await Console.Out.WriteLineAsync("Какая то ошибка...");
                        }
                    }
                }

            }
            catch (SocketException ex)
            {
                _clients.Remove(client);
                await Console.Out.WriteLineAsync(ex.Message);
            }
        }

        /// <summary>
        /// Метод для отправки сообщения всем участникам чата
        /// </summary>
        /// <param name="msg">Объект типа Message, которое нужно отослать всем участникам чата</param>
        private void SendingMessageToAllClients(UserMessage msg)
        {
            //ToDo
        }
    }
}
