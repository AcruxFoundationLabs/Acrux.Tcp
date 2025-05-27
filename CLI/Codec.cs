using System.Text;

public sealed class Codec : ITcpDataCodec<string>
{
    public string Decode(byte[] toDecode)
    {
        return Encoding.Unicode.GetString(toDecode);
    }

    public byte[] Encode(string toEncode)
    {
        return Encoding.Unicode.GetBytes(toEncode);
    }
}