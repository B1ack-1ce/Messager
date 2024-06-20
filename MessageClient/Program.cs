namespace MessageClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                MessagerClient messagerClient = new MessagerClient();

                await messagerClient.ClientStartAsync();

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
