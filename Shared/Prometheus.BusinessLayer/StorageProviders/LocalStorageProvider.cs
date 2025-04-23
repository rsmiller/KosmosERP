using Prometheus.BusinessLayer.Interfaces;
using Prometheus.Models.Interfaces;

namespace Prometheus.BusinessLayer.StorageProviders;

public class LocalStorageProvider : IStorageProvider
{
    private readonly string _LocalStoragePath;

    public LocalStorageProvider(IFileStorageSettings storageAccountSettings)
    {
        if (storageAccountSettings.local_storage_path == null)
            throw new ArgumentException("Local storage path is null");

        _LocalStoragePath = storageAccountSettings.local_storage_path;
    }

    public async Task<byte[]?> GetFileAsync(string identifier)
    {
        var filePath = Path.Combine(_LocalStoragePath, identifier);

        if (File.Exists(filePath))
            return null;

        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task<string> UploadFileAsync(byte[] data, string identifier)
    {
        var filePath = Path.Combine(_LocalStoragePath, identifier);

        if (File.Exists(filePath))
            return "";

        await File.WriteAllBytesAsync(filePath, data);

        return filePath;
    }
}
