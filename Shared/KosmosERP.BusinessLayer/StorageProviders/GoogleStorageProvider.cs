using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.Models.Interfaces;

namespace KosmosERP.BusinessLayer.StorageProviders;

public class GoogleStorageProvider : IStorageProvider
{
    public GoogleStorageProvider(IFileStorageSettings storageAccountSettings)
    {

    }

    public Task<byte[]?> GetFileAsync(string identifier)
    {
        throw new NotImplementedException();
    }

    public Task<string> UploadFileAsync(byte[] data, string identifier)
    {
        throw new NotImplementedException();
    }
}
