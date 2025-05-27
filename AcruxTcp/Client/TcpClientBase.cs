using SystemTcpClient = System.Net.Sockets.TcpClient;

public abstract class TcpClient
{
    public SystemTcpClient Client { get; }
    protected ITcpEncrypter? Encrypter { get; }

    public async Task ConnectAsync(string host, int port)
    {
        await Client.ConnectAsync(host, port);
        Console.WriteLine("Connected to server");
    }

    public void Disconnect()
    {
        Client.Close();
    }

    public TcpClient(ITcpEncrypter? encrypter)
    {
        Client = new SystemTcpClient();
        Encrypter = encrypter;
    }
}