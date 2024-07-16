

using Messenger.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Messenger
{
    public class Program
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        public static void Main(string[] args)
        {
            try
            {
                MessagerServer messagerServer = new MessagerServer();
                var serverTask = messagerServer.StartServerAsync(_cts.Token);

                Shutdown();
                Console.WriteLine("Завершение приложения!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Ошибка в методе Main");
            }
            Console.ReadKey();
        }

        private static void Shutdown()
        {
            while (true)
            {
                Console.WriteLine("Введите exit для завершения приложения...\n");
                var command = Console.ReadLine();

                if (command != null && command.ToLower().Equals("exit"))
                {
                    _cts.Cancel();
                    return;
                }
                else
                {
                    Console.WriteLine("Неопознанная команда...");
                }
            }
        }
    }
}
