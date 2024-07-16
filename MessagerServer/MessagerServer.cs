using MessagerServer.Models;
using MessagerServer.Services;
using Messenger.Data;
using Messenger.Models;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace Messenger
{
    public class MessagerServer
    {
        /// <summary>
        /// Список подключенных клиентов
        /// </summary>
        private readonly ConcurrentDictionary<string, OnlineUser> _onlineUsers = new ConcurrentDictionary<string, OnlineUser>();

        public MessagerServer()
        {
            var context = new ClientsContext();
            Console.WriteLine(context.Database.CanConnect());
        }

        /// <summary>
        /// Ожидание нового подключения (клиента)
        /// </summary>
        /// <returns>Возвращает задачу</returns>
        public async Task StartServerAsync(CancellationToken token)
        {
            var localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);
            using var listener = new TcpListener(localEndPoint);

            bool isActive = true;

            var cancelledTask = Task.Run(async () =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        isActive = false;
                        listener.Stop();

                        foreach (var user in _onlineUsers.Values)
                        {
                            user.TcpClient.Close();
                        }

                        await Console.Out.WriteLineAsync("Всех закрыли!");

                        return;
                    }
                    await Task.Delay(300);
                }
            });

            listener.Start(100);
            while (isActive)
            {
                await Console.Out.WriteLineAsync("Waiting...");
                var socket = await listener.AcceptTcpClientAsync();

                if (socket != null)
                {
                    await Console.Out.WriteLineAsync($"Подключен пользователь: {socket.Client.RemoteEndPoint}");

                    Task.Run(() => StartWorkingWithUser(socket));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task StartWorkingWithUser(TcpClient client)
        {
            OnlineUser onlineUser = null;
            try
            {
                onlineUser = await Login(client);

                Console.WriteLine($"Всего пользователей на сервере: {_onlineUsers.Count}");
                foreach (var user in _onlineUsers)
                {
                    Console.WriteLine($"Статут {user.Value.UserName} - {user.Value.TcpClient.Connected}");
                }

                if (onlineUser is not null)
                {
                    await ReceiveMessageFromCLientAsync(onlineUser);
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                await Console.Out.WriteLineAsync("Ошибка в методе LoginOrRegistrationOnServerAsync");
            }
            finally
            {
                _onlineUsers.Remove(onlineUser.Email, out onlineUser);

                Console.WriteLine($"Всего пользователей на сервере: {_onlineUsers.Count}");
                foreach (var user in _onlineUsers)
                {
                    Console.WriteLine($"Статут {user.Value.UserName} - {user.Value.TcpClient.Connected}");
                }

                //client.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<OnlineUser>? Login(TcpClient client)
        {
            var email = await ResponseRequestFromServerToUser(client, "Введите email");

            if (email is not null)
            {
                IDbService dbService = new DbService();
                var clientDto = await dbService.FindRegisteredUser(email);

                if (clientDto is not null)
                {
                    var password = await ResponseRequestFromServerToUser(client, "Введите пароль");

                    if (password is not null && password.Equals(clientDto.Password))
                    {
                        var newOnlineUser = new OnlineUser(client, email, clientDto.Username);
                        _onlineUsers.TryAdd(newOnlineUser.Email, newOnlineUser);

                        await ResponseRequestFromServerToUser(client, $"Добро пожаловать в чат {clientDto.Username}", isAwaitingReceiveMessage: false);

                        await SendingMessageWithUserInformation(newOnlineUser);

                        return newOnlineUser;
                    }
                }
                else
                {
                    return await RegistrationNewUser(client, email);
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        private async Task<OnlineUser> RegistrationNewUser(TcpClient client, string email)
        {
            var password = await ResponseRequestFromServerToUser(client, "Введите пароль");
            var userName = await ResponseRequestFromServerToUser(client, "Введите свое имя");

            if (password is not null && userName is not null)
            {
                IDbService dbService = new DbService();

                var newClient = new Client
                {
                    Email = email,
                    Name = userName,
                    Password = password,
                    ClientGuid = Guid.NewGuid()
                };

                var clientDto = await dbService.AddNewUser(newClient);

                var newOnlineUser = new OnlineUser(client, email, userName);
                _onlineUsers.TryAdd(newOnlineUser.Email, newOnlineUser);

                await ResponseRequestFromServerToUser(client, $"Вы успешно зарегистрированны.\nДобро пожаловать в чат {clientDto.Username}", isAwaitingReceiveMessage: false);
                await SendingMessageWithUserInformation(newOnlineUser);

                return newOnlineUser;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="isAwaitingReceiveMessage"></param>
        /// <returns></returns>
        private async Task<string>? ResponseRequestFromServerToUser(TcpClient client, string msg, bool isAwaitingReceiveMessage = true)
        {
            var reader = new StreamReader(client.GetStream());
            var writer = new StreamWriter(client.GetStream());
            writer.AutoFlush = true;

            var sendMessage = new Message
            {
                Username = "Server",
                Content = msg,
                DateTime = DateTime.Now
            };

            var sendContent = JsonConvert.SerializeObject(sendMessage);
            await writer.WriteLineAsync(sendContent);

            if (isAwaitingReceiveMessage)
            {
                var receiveString = await reader.ReadLineAsync();

                if (receiveString is not null)
                {
                    var receiveMessage = JsonConvert.DeserializeObject<Message>(receiveString);

                    if (receiveMessage != null)
                    {
                        return receiveMessage.Content;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return "Not waiting receive content";
            }
        }

        private async Task SendingMessageWithUserInformation(OnlineUser user)
        {
            if (user is not null)
            {
                var content = JsonConvert.SerializeObject(user);
                await user.Writer.WriteLineAsync(content);
            }
            else
            {
                await Console.Out.WriteLineAsync("Какая то ошибка в методе SendingMessageWithUserInformation");
            }
        }

        /// <summary>
        /// Асинхронный метод для пересылки сообщения всем участникам чата
        /// </summary>
        /// <param name="client">Клиент, от которого получаем сообщения.</param>
        /// <returns>Возвращает задачу (Task)</returns>
        private async Task ReceiveMessageFromCLientAsync(OnlineUser onlineUser)
        {
            try
            {
                while (true)
                {
                    var content = await onlineUser.Reader.ReadLineAsync();

                    if (content is not null)
                    {
                        // Десериализация для проверки
                        var message = JsonConvert.DeserializeObject<Message>(content);

                        await Console.Out.WriteLineAsync($"\n{message?.Username}");
                        await Console.Out.WriteLineAsync($"{message?.Content}");
                        await Console.Out.WriteLineAsync($"{message?.DateTime}");

                        foreach (var user in _onlineUsers)
                        {
                            if (!user.Key.Equals(onlineUser.Email))
                            {
                                await user.Value.Writer.WriteLineAsync(content);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return;
            }
        }
    }
}
