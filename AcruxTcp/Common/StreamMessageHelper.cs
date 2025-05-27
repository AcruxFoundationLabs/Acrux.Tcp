using System.Buffers;
using System.Net;
using System.Net.Sockets;

public static class StreamMessageHelper
{
    public static async Task<byte[]?> ReadMessageAsync(NetworkStream stream, CancellationToken token = default)
    {
        var pool = ArrayPool<byte>.Shared;
        byte[] lengthBuffer = pool.Rent(4);

        try
        {
            if (!await ReadExactAsync(stream, lengthBuffer, 4, token))
                return null;

            int length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthBuffer));
            if (length <= 0) return null;

            byte[] dataBuffer = pool.Rent(length);
            try
            {
                if (!await ReadExactAsync(stream, dataBuffer, length, token))
                    return null;

                var result = new byte[length];
                Buffer.BlockCopy(dataBuffer, 0, result, 0, length);
                return result;
            }
            finally
            {
                pool.Return(dataBuffer);
            }
        }
        finally
        {
            pool.Return(lengthBuffer);
        }
    }

    public static async Task WriteMessageAsync(NetworkStream stream, byte[] data, CancellationToken token = default)
    {
        var pool = ArrayPool<byte>.Shared;
        byte[] lengthBuffer = pool.Rent(4);
        try
        {
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length)).CopyTo(lengthBuffer, 0);
            await stream.WriteAsync(lengthBuffer.AsMemory(0, 4), token);
            await stream.WriteAsync(data.AsMemory(0, data.Length), token);
        }
        finally
        {
            pool.Return(lengthBuffer);
        }
    }

    private static async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, int size, CancellationToken token)
    {
        int read = 0;
        while (read < size)
        {
            int r = await stream.ReadAsync(buffer.AsMemory(read, size - read), token);
            if (r == 0) return false; // Disconnected
            read += r;
        }
        return true;
    }
}
