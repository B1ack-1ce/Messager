

namespace MessagerServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MessagerServer messagerServer = new MessagerServer();

                var serverTask = Task.Run(messagerServer.StartServerAsync);

                serverTask.Wait();

                Console.WriteLine("Завершение работы сервера...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}
