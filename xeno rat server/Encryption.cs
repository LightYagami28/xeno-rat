using System;
using System.IO;
using System.Security.Cryptography;

namespace XenoRatServer
{
    public static class Encryption
    {
        public static byte[] Encrypt(byte[] data, byte[] key)
        {
            byte[] encrypted;
            byte[] IV = new byte[16];
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = IV;
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return encrypted;
        }

        public static byte[] Decrypt(byte[] data, byte[] key)
        {
            byte[] IV = new byte[16];
            byte[] decrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = IV;
                using (MemoryStream msDecrypt = new MemoryStream(data))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            csDecrypt.CopyTo(ms);
                            decrypted = ms.ToArray();
                        }
                    }
                }
            }
            return decrypted;
        }
    }
}
