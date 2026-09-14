using Amazon;
using Amazon.S3;
using MySqlX.XDevAPI;
using KosmosERP.BusinessLayer.Interfaces;
using KosmosERP.Models.Interfaces;
using Amazon.S3.Model;

namespace KosmosERP.BusinessLayer.StorageProviders;

public class AWSStorageProvider : IStorageProvider
{
    private readonly string _AccessKey;
    private readonly string _SecretKey;
    private readonly string _BucketName;
    private readonly RegionEndpoint _Region;

    public AWSStorageProvider(IFileStorageSettings storageAccountSettings)
    {
        if(storageAccountSettings.aws_access_key == null)
            throw new ArgumentException("AWS access key is null");
        if (storageAccountSettings.aws_secret_key == null)
            throw new ArgumentException("AWS secret key is null");
        if (storageAccountSettings.aws_bucket_name == null)
            throw new ArgumentException("AWS bucket name is null");

        _AccessKey = storageAccountSettings.aws_access_key;
        _SecretKey = storageAccountSettings.aws_secret_key;
        _BucketName = storageAccountSettings.aws_bucket_name;
        
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

    public async Task<byte[]?> GetFileAsync(string identifier)
    {
        var client = new AmazonS3Client(_AccessKey, _SecretKey, _Region);

        var response = await client.GetObjectAsync(new GetObjectRequest
        {
            BucketName = _BucketName,
            Key = identifier
        });

        using(var mem_stream = new MemoryStream())
        {
            
            await response.ResponseStream.CopyToAsync(mem_stream);
            mem_stream.Position = 0;
            
            return mem_stream.ToArray();
        }
    }

    public async Task<string> UploadFileAsync(byte[] data, string identifier)
    {
        var client = new AmazonS3Client(_AccessKey, _SecretKey, _Region);
        
        var response = await client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _BucketName,
            Key = identifier,
            InputStream = new MemoryStream(data)
        });

        var region = _Region.SystemName;
        return identifier;
    }   
}
