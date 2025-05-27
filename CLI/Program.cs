using System;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "server")
        {
            var server = new TcpServer<string, string>(5000, new RequestHandler(), new TcpEncrypter());
            var cts = new CancellationTokenSource();
            await server.StartAsync(cts.Token);
        }
        else if (args.Length > 0 && args[0] == "client")
        {
            var client = new TcpClient<string, string>(new Codec(), new Codec(), new TcpEncrypter());
            await client.ConnectAsync("127.0.0.1", 5000);

            while (true)
            {
                Console.Write("Enter message (or 'exit'): ");
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || input.ToLower() == "exit") break;

                string response = await client.SendRequestAsync(input);
                Console.WriteLine($"[SERVER]: {response}");
            }

            client.Disconnect();
        }
        else
        {
            Console.WriteLine("Error: Specify first argument as 'server' or 'client'.");
        }
    }
}
