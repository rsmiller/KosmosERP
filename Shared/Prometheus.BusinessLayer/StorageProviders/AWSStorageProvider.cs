using Amazon;
using Amazon.S3;
using MySqlX.XDevAPI;
using Prometheus.BusinessLayer.Interfaces;
using Prometheus.Models.Interfaces;

namespace Prometheus.BusinessLayer.StorageProviders;

public class AWSStorageProvider : IStorageProvider
{
    private readonly string _AccessKey;
    private readonly string _SecretKey;
    private readonly RegionEndpoint _Region;

    public AWSStorageProvider(IFileStorageSettings storageAccountSettings)
    {
        if(storageAccountSettings.aws_access_key == null)
            throw new ArgumentException("AWS access key is null");
        if (storageAccountSettings.aws_secret_key == null)
            throw new ArgumentException("AWS secret key is null");


        _AccessKey = storageAccountSettings.aws_access_key;
        _SecretKey = storageAccountSettings.aws_secret_key;

        switch(storageAccountSettings.aws_region)
        {
            case "us-east-1":
                _Region = RegionEndpoint.USEast1;
                break;
            case "us-east-2":
                _Region = RegionEndpoint.USEast2;
                break;
            case "us-west-1":
                _Region = RegionEndpoint.USWest1;
                break;
            case "us-west-2":
                _Region = RegionEndpoint.USWest2;
                break;
            default:
                throw new ArgumentException("Invalid AWS region specified.");
        }
    }

    public Task<byte[]?> GetFileAsync(string identifier)
    {
        //var client = new AmazonS3Client(_AccessKey, _SecretKey, _Region);
        throw new NotImplementedException();
    }

    public Task<bool> UploadFileAsync(byte[] data, string identifier)
    {
        //var client = new AmazonS3Client(_AccessKey, _SecretKey, _Region);
        throw new NotImplementedException();
    }
}
