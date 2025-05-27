public interface ITcpDataCodec<T>
{
    T Decode(byte[] toDecode);
    byte[] Encode(T toEncode);
}