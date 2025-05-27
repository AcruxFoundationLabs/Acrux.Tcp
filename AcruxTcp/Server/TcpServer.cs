using System.Net;
using System.Net.Sockets;

public sealed class TcpServer<TRequest, TResponse> : TcpServer
{
    private readonly IRequestHadler<TRequest, TResponse> _requestHandler;

    protected override async Task HandleClientAsync(System.Net.Sockets.TcpClient client, CancellationToken token)
    {
        using var stream = client.GetStream();
        Console.WriteLine("Client connected");

        try
        {
            while (!token.IsCancellationRequested)
            {
                var clientIp = ((IPEndPoint)client.Client.RemoteEndPoint!).Address.MapToIPv4();
                var serverIp = ((IPEndPoint)client.Client.LocalEndPoint!).Address.MapToIPv4();

                byte[]? requestBytes = await StreamMessageHelper.ReadMessageAsync(stream, token);
                #if DEBUG
                Console.WriteLine($"[IN ] Request bytes: {BitConverter.ToString(requestBytes ?? [])}.");
                #endif

                if (requestBytes is null)
                {
                    #if DEBUG
                    Console.WriteLine($"[INFO] Client disconected, read resulted in null byte array reference.");
                    #endif
                    break;
                }

                if(Encrypter is not null)
                {
                    requestBytes = Encrypter.Decrypt(requestBytes, clientIp, serverIp);

                    #if DEBUG
                    Console.WriteLine($"[IN ] Request bytes decrypted: {BitConverter.ToString(requestBytes)}.");
                    #endif
                }


                TRequest request = _requestHandler.RequestCodec.Decode(requestBytes);
                #if DEBUG
                Console.WriteLine($"[IN ] Decoded request into '{typeof(TRequest)}': '{request}'.");
                #endif
                
                TResponse response = await _requestHandler.HandleRequestAsync(request);
                #if DEBUG
                Console.WriteLine($"[OUT] Created response as '{typeof(TResponse)}': '{response}'.");
                #endif
                
                byte[] responseBytes = _requestHandler.ResponseCodec.Encode(response);
                #if DEBUG
                Console.WriteLine($"[OUT] Encoded response: {BitConverter.ToString(responseBytes)}.");
                #endif

                if(Encrypter is not null)
                {
                    responseBytes = Encrypter.Encrypt(responseBytes, clientIp, serverIp);

                    #if DEBUG
                    Console.WriteLine($"[OUT] Response bytes encrypted: {BitConverter.ToString(responseBytes)}.");
                    #endif
                }


                await StreamMessageHelper.WriteMessageAsync(stream, responseBytes, token);
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Connection lost.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception handling client: {e}.");
        }
        finally
        {
            client.Close();
            Console.WriteLine("Client disconnected");
        }
    }

    public TcpServer(int port, IRequestHadler<TRequest, TResponse> requestHadler, ITcpEncrypter? encrypter = null)
        : base(port, encrypter)
    {
        _requestHandler = requestHadler;
    }
}
