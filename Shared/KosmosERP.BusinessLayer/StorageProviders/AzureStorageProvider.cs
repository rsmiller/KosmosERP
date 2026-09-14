using System.Security.Policy;
using Azure.Storage.Blobs;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.StorageProviders;

public class AzureStorageProvider : IStorageProvider
{
    private readonly string _ConnectionString;
    private readonly string _ContainerName;

    public AzureStorageProvider(IFileStorageSettings storageAccountSettings)
    {
        if (storageAccountSettings.azure_connection_string == null)
            throw new ArgumentException("Azure connection string is null");
        if (storageAccountSettings.azure_container_name == null)
            throw new ArgumentException("Azure container name is null");

        _ConnectionString = storageAccountSettings.azure_connection_string;
        _ContainerName = storageAccountSettings.azure_container_name;
    }

    public async Task<byte[]?> GetFileAsync(string identifier)
    {   
        var serviceClient = new BlobServiceClient(_ConnectionString);
        var containerClient = serviceClient.GetBlobContainerClient(_ContainerName);
        BlobClient? blobClient = null;

        if(Uri.IsWellFormedUriString(identifier, UriKind.Absolute))
        {
            int container_pos = identifier.IndexOf(_ContainerName);
            int guid_start = identifier.IndexOf("/", container_pos + 1);
            string blob_name = identifier.Substring(guid_start + 1);

            blobClient = containerClient.GetBlobClient(blob_name);
        }
        else
        {
            blobClient = containerClient.GetBlobClient(identifier);
        }

        var download = await blobClient.DownloadAsync();
        var download_info = download.Value;

        using (var buffer_stream = new MemoryStream())
        {
            await download_info.Content.CopyToAsync(buffer_stream);

            buffer_stream.Position = 0;

            return buffer_stream.ToArray();
        }  
        
    }

    public async Task<string> UploadFileAsync(byte[] data, string identifier)
    {
        var serviceClient = new BlobServiceClient(_ConnectionString);
        var containerClient = serviceClient.GetBlobContainerClient(_ContainerName);
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(identifier);
        var response = await blobClient.UploadAsync(new BinaryData(data));
        
        return blobClient.Uri.AbsoluteUri;
    }
}
