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
        /// Начало работы сервера
        /// </summary>
        /// <returns>Возвращает задачу (Task)</returns>
        public async Task StartServerAsync()
        {
            while (true)
            {
                var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);
                using var listener = new TcpListener(localEndPoint);
                listener.Start(100);

                await Console.Out.WriteLineAsync("Waiting...");
                var socket = await listener.AcceptTcpClientAsync();

                if (socket != null)
                {
                    await Console.Out.WriteLineAsync($"Подключен новый пользователь: {socket.Client.RemoteEndPoint}");

                    Task.Run(() => ReceiveUserMessage(socket));
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

                using (var stream = client.GetStream())
                {
                    while (true)
                    {
                        byte[] buffer = new byte[2048];
                        await stream.Socket.ReceiveAsync(buffer);

                        content = Encoding.UTF8.GetString(buffer);
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
                        else
                        {
                            throw new SocketException(0, "Клиент разорвал подключение.");
                        }
                        // Временное решение уведомления клиента, от которого пришло сообщение. Написано грязно
                        await stream.Socket
                            .SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject
                            (
                                new RecieveMessage 
                                { 
                                    DateTime = DateTime.Now,
                                    UsernameFrom = "Server",
                                    Content = "Сообщение получено на сервер."
                                }
                                )));
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
