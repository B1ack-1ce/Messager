

namespace MessagerServer
{
    internal class Program
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        static void Main(string[] args)
        {
            try
            {
                MessagerServer messagerServer = new MessagerServer();

                var serverTask = Task.Run(() => messagerServer.StartServerAsync(_cts.Token));

                Shutdown();

                serverTask.Wait();

                Console.WriteLine("Завершение приложения!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
