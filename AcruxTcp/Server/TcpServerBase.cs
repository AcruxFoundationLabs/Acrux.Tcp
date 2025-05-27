using System.Net;
using System.Net.Sockets;
using System.Text;

public abstract class TcpServer
{
    public TcpListener Listener { get; }
    protected ITcpEncrypter? Encrypter { get; }

    public async Task StartAsync(CancellationToken token = default)
    {
        Listener.Start();
        Console.WriteLine("Server started...");

        while (!token.IsCancellationRequested)
        {
            var client = await Listener.AcceptTcpClientAsync(token);
            _ = HandleClientAsync(client, token); // Fire and forget
        }
    }

    public void Stop()
    {
        Listener.Stop();
    }

    protected abstract Task HandleClientAsync(System.Net.Sockets.TcpClient client, CancellationToken token);

    public TcpServer(int port, ITcpEncrypter? encrypter)
    {
        Listener = new TcpListener(IPAddress.Any, port);
        Encrypter = encrypter;
    }
}