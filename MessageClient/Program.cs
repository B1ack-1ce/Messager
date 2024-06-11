namespace MessageClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new MessagerClient().ClientStart();
        }
    }
}
