/*
 * Copyright 2016 Matthew Cosand
 */
namespace Kcsara.Database.Web.Services
{
  using System;
  using System.IO;
  using System.Linq;
  using System.Security.Cryptography;

  public interface IEncryptionService
  {
    string Encrypt(string unprotextedText);
    string Decrypt(string protectedText);
  }

  public class EncryptionService : IEncryptionService
  {
    private readonly byte[] key;

    public EncryptionService(string base64Key)
    {
      key = Convert.FromBase64String(base64Key);
    }

    public string Encrypt(string text)
    {
      if (string.IsNullOrWhiteSpace(text)) return text;

      using (var aes = Aes.Create())
      {
        aes.GenerateIV();
        aes.Key = key;
        byte[] encrypted;

        // Create a decrytor to perform the stream transform.
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        // Create the streams used for encryption.
        using (MemoryStream msEncrypt = new MemoryStream())
        {
          using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
          {
            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
            {
              //Write all data to the stream.
              swEncrypt.Write(text);
            }
            encrypted = msEncrypt.ToArray();
          }
        }

        byte[] chunk = new byte[1 + aes.IV.Length + encrypted.Length];
        chunk[0] = (byte)aes.IV.Length;
        Buffer.BlockCopy(aes.IV, 0, chunk, 1, aes.IV.Length);
        Buffer.BlockCopy(encrypted, 0, chunk, aes.IV.Length + 1, encrypted.Length);
        return Convert.ToBase64String(chunk);
      }
    }

    public string Decrypt(string encrypted)
    {
      if (string.IsNullOrWhiteSpace(encrypted)) return encrypted;

      using (var aes = Aes.Create())
      {
        var encryptedBytes = Convert.FromBase64String(encrypted);

        aes.IV = encryptedBytes.Skip(1).Take(encryptedBytes[0]).ToArray();
        aes.Key = key;

        // Create a decrytor to perform the stream transform.
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        // Create the streams used for decryption.
        using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes.Skip(1 + aes.IV.Length).ToArray()))
        {
          using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
          {
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
              // Read the decrypted bytes from the decrypting stream
              // and place them in a string.
              return srDecrypt.ReadToEnd();
            }
          }
        }
      }
    }
  }
}
