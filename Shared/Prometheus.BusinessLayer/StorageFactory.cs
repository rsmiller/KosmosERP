using Prometheus.BusinessLayer.Interfaces;
using Prometheus.BusinessLayer.StorageProviders;
using Prometheus.Models;
using Prometheus.Models.Interfaces;

namespace Prometheus.BusinessLayer;

public static class StorageFactory
{
    public static IStorageProvider Create(IFileStorageSettings storageAccountSettings)
    {
        if (string.IsNullOrEmpty(storageAccountSettings.account_provider))
            throw new ArgumentException("Storage account provider is null or empty");


        if (storageAccountSettings.account_provider.Equals(StorageType.Azure, StringComparison.OrdinalIgnoreCase))
        {
            return new AzureStorageProvider(storageAccountSettings);
        }
        else if (storageAccountSettings.account_provider.Equals(StorageType.AWS, StringComparison.OrdinalIgnoreCase))
        {
            return new AWSStorageProvider(storageAccountSettings);
        }
        else if (storageAccountSettings.account_provider.Equals(StorageType.Google, StringComparison.OrdinalIgnoreCase))
        {
            return new GoogleStorageProvider(storageAccountSettings);
        }
        else if (storageAccountSettings.account_provider.Equals(StorageType.Local, StringComparison.OrdinalIgnoreCase))
        {
            return new LocalStorageProvider(storageAccountSettings);
        }
        else
        {
            throw new ArgumentNullException("Storage account provider not supported.");
        }
    }
}
