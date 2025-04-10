using Prometheus.BusinessLayer.Interfaces;
using Prometheus.Models.Interfaces;

namespace Prometheus.BusinessLayer.StorageProviders;

public class GoogleStorageProvider : IStorageProvider
{
    public GoogleStorageProvider(IFileStorageSettings storageAccountSettings)
    {

    }

    public Task<byte[]?> GetFileAsync(string identifier)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UploadFileAsync(byte[] data, string identifier)
    {
        throw new NotImplementedException();
    }
}
