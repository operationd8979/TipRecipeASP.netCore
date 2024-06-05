using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Serilog;

namespace TipRecipe.Services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _accountKey;
        private Dictionary<string, string> _sasTokens = new Dictionary<string, string>();
        private List<string> _containers = new List<string>();

        public AzureBlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _accountKey = connectionString.Split(';').FirstOrDefault(x => x.StartsWith("AccountKey="))!;
            _accountKey = _accountKey.Substring(_accountKey.IndexOf('=')+1);
        }

        public async Task SetContainerPublicAccessAsync(string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
        }

        public async Task<List<string>> GetAllContainersAsync()
        {
            var containers = new List<string>();
            await foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainersAsync())
            {
                await foreach (BlobItem blob in _blobServiceClient.GetBlobContainerClient(container.Name).GetBlobsAsync())
                {
                    containers.Add($"{container.Name}/{blob.Name}");
                }
            }
            return containers;
        }

        public async Task<string> UploadFileAsync(string containerName, string blobName, Stream fileStream)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, true);

            if(_sasTokens.TryGetValue(containerName,out var sasToken))
            {
                return $"{blobClient.Uri}?{sasToken}";
            }
            sasToken = GenerateContainerSasToken(containerName);
            return $"{blobClient.Uri}?{sasToken}";
        }

        private string GenerateSasToken(BlobClient blobClient)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            return sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(blobClient.AccountName, _accountKey)).ToString();
        }

        private string GenerateContainerSasToken(string containerName)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Resource = "c",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(3)
            };
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);
            string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_blobServiceClient.AccountName, _accountKey)).ToString();
            if(!_sasTokens.ContainsKey(containerName))
            {
                _sasTokens.Add(containerName, sasToken);
                _containers.Add(containerName);
            }
            else
            {
                _sasTokens[containerName] = sasToken;
            }
            return sasToken;
        }

        public void UpdateSasTokensForContainers()
        {
            //if(_containers.Count == 0)
            //{
            //    foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainers())
            //    {
            //        _containers.Add(container.Name);
            //        Log.Information(container.Name);
            //    }
            //}
            foreach(string container in _containers)
            {
                GenerateContainerSasToken(container);
                Log.Information("update "+container);
            }
        }

        public async Task<Stream> DownloadFileAsync(string containerName, string blobName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadAsync();
            return response.Value.Content;
        }
    }
}
