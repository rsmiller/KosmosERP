namespace Prometheus.BusinessLayer.Interfaces;
public interface IStorageProvider
{
    Task<byte[]?> GetFileAsync(string identifier);
    Task<bool> UploadFileAsync(byte[] data, string identifier);
}
