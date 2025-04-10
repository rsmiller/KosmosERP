using Azure.Storage.Blobs;
using Microsoft.Extensions.Azure;
using Prometheus.BusinessLayer.Interfaces;
using Prometheus.Models.Interfaces;

namespace Prometheus.BusinessLayer.StorageProviders;

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
        _ConnectionString = storageAccountSettings.azure_container_name;
    }

    public Task<byte[]?> GetFileAsync(string identifier)
    {
        var serviceClient = new BlobServiceClient(_ConnectionString);
        var containerClient = serviceClient.GetBlobContainerClient(_ContainerName);
        
        throw new NotImplementedException();
    }

    public Task<bool> UploadFileAsync(byte[] data, string identifier)
    {
        var serviceClient = new BlobServiceClient(_ConnectionString);
        var containerClient = serviceClient.GetBlobContainerClient(_ContainerName);
        //serviceClient.AddBlobServiceClient()

        throw new NotImplementedException();
    }
}
