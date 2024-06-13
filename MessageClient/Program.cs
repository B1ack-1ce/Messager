namespace MessageClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                MessagerClient messagerServer = new MessagerClient();

                await messagerServer.ClientStartAsync();

                Console.WriteLine("Завершение работы клиента...");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}
