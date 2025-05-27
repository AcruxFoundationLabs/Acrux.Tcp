using System.Net;

public interface ITcpEncrypter
{
    byte[] Encrypt(byte[] original, IPAddress clientIp, IPAddress serverIp);
    byte[] Decrypt(byte[] original, IPAddress clientIp, IPAddress serverIp);
}