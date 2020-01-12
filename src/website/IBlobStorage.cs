using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;

namespace Sar.Database.Services
{
  public interface IBlobStorage
  {
    Task Download(string path, Stream target);
  }

  public class AzureBlobStorage : IBlobStorage
  {
    private readonly BlobContainerClient containerClient;

    public AzureBlobStorage(string connectionString, string containerName)
    {
      BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
      containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<bool> Exists(string path)
    {
      var client = GetClient(path);
      return await client.ExistsAsync();
    }

    public async Task Download(string path, Stream target)
    {
      BlobClient blobClient = GetClient(path);
      BlobDownloadInfo download = await blobClient.DownloadAsync();
      await download.Content.CopyToAsync(target);
    }

    private BlobClient GetClient(string path)
    {
      BlobClient blobClient = containerClient.GetBlobClient(path);
      return blobClient;
    }
  }
}
