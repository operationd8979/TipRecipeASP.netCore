using Amazon.SecretsManager.Model;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Serilog;
using System.Reflection.Metadata;

namespace TipRecipe.Services
{
    public class AzureBlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _accountKey;
        private readonly string _bucketName = "test";

        private List<string> _containers;

        //sas include policies
        static readonly string POLICY_READONLY = "readOnly";
        static readonly string POLICY_WRITEONLY = "writeOnly";
        static readonly string POLICY_READWRITE = "readWrite";
        static readonly List<string> _policies = new List<string>() { POLICY_READONLY, POLICY_WRITEONLY, POLICY_READWRITE };

        public AzureBlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _accountKey = connectionString.Split(';').FirstOrDefault(x => x.StartsWith("AccountKey="))!;
            _accountKey = _accountKey.Substring(_accountKey.IndexOf('=')+1);
            _containers = new List<string>();
            this.InitIncludePolicy();
        }

        private void InitIncludePolicy()
        {
            foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainers())
            {
                if (container.Name.StartsWith(_bucketName))
                {
                    _containers.Add(container.Name);
                }
            }
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

        public async Task<string> UploadFileAsync(string containerName, string blobName, Stream fileStream, Dictionary<string, string> tags)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, true);
            await blobClient.SetTagsAsync(tags);
            return blobClient.Uri.ToString();
        }

        public string GenerateSasTokenPolicy(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobClient = new BlobClient(uri);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                Identifier = POLICY_READONLY
            };
            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_blobServiceClient.AccountName, _accountKey)).ToString();
            
            return $"{blobClient.Uri}?{sasToken}";
        }

        public string GenerateSasToken(string blobUrl)
        {
            var uri = new Uri(blobUrl);
            var blobClient = new BlobClient(uri);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_blobServiceClient.AccountName, _accountKey)).ToString();
            return $"{blobClient.Uri}?{sasToken}";
        }


        private async Task CreateStoredAccessPolicyAsync(BlobContainerClient containerClient)
        {
            BlobSignedIdentifier[] blobSignedIdentifiers = new BlobSignedIdentifier[3];
            int i = 0;
            foreach(var policy in _policies)
            {
                BlobSignedIdentifier identifier = new BlobSignedIdentifier
                {
                    Id = policy,
                    AccessPolicy = new BlobAccessPolicy
                    {
                        StartsOn = DateTimeOffset.UtcNow,
                        ExpiresOn = DateTimeOffset.UtcNow.AddHours(12),
                    }
                };
                if (policy.Equals(POLICY_READONLY))
                {
                    identifier.AccessPolicy.Permissions = "r";
                }
                else if (policy.Equals(POLICY_WRITEONLY))
                {
                    identifier.AccessPolicy.Permissions = "w";
                }
                else if (policy.Equals(POLICY_READWRITE))
                {
                    identifier.AccessPolicy.Permissions = "rw";
                }
                blobSignedIdentifiers[i++] = identifier;
            }
            await containerClient.SetAccessPolicyAsync(permissions: blobSignedIdentifiers);
        }

        private async Task UpdateStoredAccessPolicyAsync(BlobContainerClient containerClient, string policyName, BlobAccessPolicy newPolicy)
        {
            var existingPolicies = await containerClient.GetAccessPolicyAsync();
            var policies = existingPolicies.Value.SignedIdentifiers.ToList();

            BlobSignedIdentifier? identifier = policies.FirstOrDefault(p => p.Id == policyName);
            if (identifier is not null)
            {
                identifier.AccessPolicy = newPolicy;
                await containerClient.SetAccessPolicyAsync(permissions: policies);
            }
        }

        private async Task DeleteStoredAccessPolicyAsync(BlobContainerClient containerClient, string policyName)
        {
            var existingPolicies = await containerClient.GetAccessPolicyAsync();
            var policies = existingPolicies.Value.SignedIdentifiers.ToList();

            var policyToRemove = policies.FirstOrDefault(p => p.Id == policyName);
            if (policyToRemove != null)
            {
                policies.Remove(policyToRemove);
                await containerClient.SetAccessPolicyAsync(permissions: policies.ToArray());
            }
        }

        public async Task UpdateSasTokensForContainers()
        {
            foreach (string container in _containers)
            {
                await CreateStoredAccessPolicyAsync(_blobServiceClient.GetBlobContainerClient(container));
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
