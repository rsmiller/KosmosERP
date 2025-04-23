namespace Prometheus.BusinessLayer.Interfaces;
public interface IStorageProvider
{
    Task<byte[]?> GetFileAsync(string identifier);
    Task<string> UploadFileAsync(byte[] data, string identifier);
}
