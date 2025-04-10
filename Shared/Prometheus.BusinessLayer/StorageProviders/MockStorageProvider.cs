using Prometheus.BusinessLayer.Interfaces;
using Prometheus.Models.Interfaces;
using System.Text;

namespace Prometheus.BusinessLayer.StorageProviders;

public class MockStorageProvider : IStorageProvider
{
    private readonly string _LocalStoragePath;

    public MockStorageProvider(IFileStorageSettings storageAccountSettings)
    {

    }

    public async Task<byte[]?> GetFileAsync(string identifier)
    {
        string test = "Hello";

        return Encoding.UTF8.GetBytes(test);
    }

    public async Task<bool> UploadFileAsync(byte[] data, string identifier)
    {
        return true;
    }
}
