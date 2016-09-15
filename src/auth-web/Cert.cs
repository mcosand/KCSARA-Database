using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Sar.Database.Web.Auth
{
  public static class Cert
  {
    public static X509Certificate2 Load(string keyPhrase, Action<string> logWarn)
    {
      var assembly = typeof(Cert).Assembly;
      var certFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cert.pfx");

      bool useTest = !File.Exists(certFilePath) || string.IsNullOrWhiteSpace(keyPhrase);
      if (useTest)
      {
        logWarn($"Signing certificate not setup {certFilePath}. Using test certificate");
      }

      // If no signing cert is available use the test cert.
      using (var stream = useTest
        ? assembly.GetManifestResourceStream("Sar.Database.Web.Auth.idsrv3test.pfx")
        : new FileStream(certFilePath, FileMode.Open, FileAccess.Read))
      {
        return new X509Certificate2(ReadStream(stream), keyPhrase ?? "idsrv3test");
      }
    }

    private static byte[] ReadStream(Stream input)
    {
      var buffer = new byte[16 * 1024];
      using (var ms = new MemoryStream())
      {
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
          ms.Write(buffer, 0, read);
        }
        return ms.ToArray();
      }
    }
  }
}