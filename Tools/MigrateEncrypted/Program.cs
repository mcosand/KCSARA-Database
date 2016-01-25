using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Kcsar.Database.Model;

namespace MigrateEncrypted
{
  class Program
  {
    public const string MEMBER_SENSITIVE = "Member-Sensitive-Data";

    private static byte[] encryptKey = Convert.FromBase64String(ConfigurationManager.AppSettings["encryptKey"]);
    private static Aes aes = Aes.Create();

    static void Main(string[] args)
    {
      using (var db = new KcsarContext())
      {
        db.SystemUpdates = true;
        foreach (var row in db.Members.Select(f => f.MedicalInfo).Where(f => f.EncryptionType == EncryptionType.MachineKey))
        {
          MigrateEncryption(row, row.EncryptedAllergies, s => row.EncryptedAllergies = s);
          MigrateEncryption(row, row.EncryptedDisclosures, s => row.EncryptedDisclosures = s);
          MigrateEncryption(row, row.EncryptedMedications, s => row.EncryptedMedications = s);
          if (row.EncryptionType == EncryptionType.MachineKey)
          {
            ((IObjectContextAdapter)db).ObjectContext.DeleteObject(row);
          }
        }

        foreach (var row in db.Members.SelectMany(f => f.EmergencyContacts).Where(f => f.EncryptionType == EncryptionType.MachineKey))
        {
          MigrateEncryption(row, row.EncryptedData, f => row.EncryptedData = f);
          if (row.EncryptionType == EncryptionType.MachineKey)
          {
            ((IObjectContextAdapter)db).ObjectContext.DeleteObject(row);
          }
        }
        db.SaveChanges();
      }
      aes.Dispose();
    }

    private static void MigrateEncryption(MemberEmergencyContactRow row, string text, Action<string> update)
    {
      MigrateEncryption(text, s => { update(s); row.EncryptionType = EncryptionType.ManagedAES; });
    }

    private static void MigrateEncryption(MemberMedicalRow row, string text, Action<string> update)
    {
      MigrateEncryption(text, s => { update(s); row.EncryptionType = EncryptionType.ManagedAES; });
    }

    private static void MigrateEncryption(string text, Action<string> update)
    {
      string clearText = DecryptMachineKey(text);
      if (!string.IsNullOrWhiteSpace(clearText))
      {
        Console.WriteLine(clearText);
        update(EncryptAes(clearText));
      }
    }

    private static string DecryptMachineKey(string encrypted)
    {
      if (string.IsNullOrWhiteSpace(encrypted)) return null;

      var protectedBytes = Convert.FromBase64String(encrypted);
      var unprotectedBytes = MachineKey.Unprotect(protectedBytes, MEMBER_SENSITIVE);
      return Encoding.UTF8.GetString(unprotectedBytes);
    }

    private static string EncryptAes(string text)
    {
      aes.GenerateIV();
      aes.Key = encryptKey;
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

    private static string DecryptAes(string encrypted)
    {
      if (string.IsNullOrWhiteSpace(encrypted)) return encrypted;

      var encryptedBytes = Convert.FromBase64String(encrypted);

      aes.IV = encryptedBytes.Skip(1).Take(encryptedBytes[0]).ToArray();
      aes.Key = encryptKey;

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
