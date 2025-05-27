using System.Net;
using SystemTcpClient = System.Net.Sockets.TcpClient;

public sealed class TcpClient<TRequest, TResponse> : TcpClient
{
    private readonly ITcpDataCodec<TRequest> _requestCodec;
    private readonly ITcpDataCodec<TResponse> _responseCodec;

    public TcpClient(ITcpDataCodec<TRequest> requestCodec, ITcpDataCodec<TResponse> responseCodec, ITcpEncrypter? encrypter = null)
        : base(encrypter)
    {
        _requestCodec = requestCodec;
        _responseCodec = responseCodec;
    }

    public async Task<TResponse> SendRequestAsync(TRequest request, CancellationToken token = default)
    {
        var clientIp = ((IPEndPoint)Client.Client.LocalEndPoint!).Address.MapToIPv4();
        var serverIp = ((IPEndPoint)Client.Client.RemoteEndPoint!).Address.MapToIPv4();

        #if DEBUG
        Console.WriteLine($"[OUT] Request as '{typeof(TRequest)}': '{request}'.");
        #endif
        
        var stream = Client.GetStream();

        byte[] requestBytes = _requestCodec.Encode(request);
        #if DEBUG
        Console.WriteLine($"[OUT] Request bytes: {BitConverter.ToString(requestBytes)}.");
        #endif

        if(Encrypter is not null)
        {
            requestBytes = Encrypter.Encrypt(requestBytes, clientIp, serverIp);

            #if DEBUG
            Console.WriteLine($"[OUT] Request bytes encrypted: {BitConverter.ToString(requestBytes)}.");
            #endif
        }
        
        await StreamMessageHelper.WriteMessageAsync(stream, requestBytes, token);

        byte[]? responseBytes = await StreamMessageHelper.ReadMessageAsync(stream, token);
        #if DEBUG
        Console.WriteLine($"[IN ] Response bytes: {BitConverter.ToString(responseBytes ?? [])}.");
        #endif
        
        if(Encrypter is not null && responseBytes is not null)
        {
            responseBytes = Encrypter.Decrypt(responseBytes, clientIp, serverIp);
        
            #if DEBUG
            Console.WriteLine($"[IN ] Response bytes decrypted: {BitConverter.ToString(responseBytes ?? [])}");
            #endif
        }

        if (responseBytes == null) throw new Exception("Server error.");

        TResponse response = _responseCodec.Decode(responseBytes);
        #if DEBUG
        Console.WriteLine($"[IN ] Response decoded as '{typeof(TResponse)}': '{response}'.");
        #endif

        return response;
    }
}
