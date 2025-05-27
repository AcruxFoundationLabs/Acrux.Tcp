using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

internal sealed class TcpEncrypter : ITcpEncrypter
{
    private static byte[] GenerateKeyFromIP(string ipAddress)
    {
        using (SHA256 sha = SHA256.Create())
        {
            return sha.ComputeHash(Encoding.UTF8.GetBytes(ipAddress));
        }
    }

    public byte[] Encrypt(byte[] original, IPAddress clientIp, IPAddress serverIp)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = GenerateKeyFromIP(clientIp.ToString());
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            byte[] encrypted = encryptor.TransformFinalBlock(original, 0, original.Length);

            // Prepend IV to ciphertext
            byte[] result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

            return result;
        }
    }

    public byte[] Decrypt(byte[] original, IPAddress clientIp, IPAddress serverIp)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = GenerateKeyFromIP(clientIp.ToString());
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Extract IV from the beginning of the ciphertext
            byte[] iv = new byte[16];
            Buffer.BlockCopy(original, 0, iv, 0, iv.Length);
            aes.IV = iv;

            byte[] encryptedData = new byte[original.Length - iv.Length];
            Buffer.BlockCopy(original, iv.Length, encryptedData, 0, encryptedData.Length);

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
        }
    }
}
