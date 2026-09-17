namespace KosmosERP.Models.Interfaces;
public interface IStorageProviderdd
{
    Task<byte[]?> GetFileAsync(string identifier);
    Task<string> UploadFileAsync(byte[] data, string identifier);
}
