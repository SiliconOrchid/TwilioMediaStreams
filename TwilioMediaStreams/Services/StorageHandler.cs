using System;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using TwilioMediaStreams.Models;

namespace TwilioMediaStreams.Services
{
    public static class StorageHandler
    {
        public static async Task<CloudBlockBlob> SetupCloudStorageAsync(ProjectSettings projectSettings)
        {
            // new random filename
            string fileName = $"{Guid.NewGuid()}.wav";

            // set container name
            var containerName = projectSettings.AzureStorageAccountContainerName;

            // create storage account object
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(projectSettings.AzureStorageAccountConnectionString);

            // create storage account client
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            // create reference of storage account container
            CloudBlobContainer container = client.GetContainerReference(containerName);

            // create container if it doesn't already exist
            var isCreated = await container.CreateIfNotExistsAsync();

            // set the permissions to blob
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

            // MIME type used for MULAW wav files
            blob.Properties.ContentType = "audio/wav";  

            return blob;
        }
    }
}
