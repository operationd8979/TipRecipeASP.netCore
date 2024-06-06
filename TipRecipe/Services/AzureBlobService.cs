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
        private readonly string _bucketName = "test";

        private List<string> _containers;

        //sas include policies
        static readonly string POLICY_READONLY = "readOnly";
        static readonly string POLICY_WRITEONLY = "writeOnly";
        static readonly string POLICY_READWRITE = "readWrite";
        static readonly List<string> _policies = new List<string>() { POLICY_READONLY, POLICY_WRITEONLY, POLICY_READWRITE };

        //sas noninclude policies
        private Dictionary<string, string> _sasTokens;

        public AzureBlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _accountKey = connectionString.Split(';').FirstOrDefault(x => x.StartsWith("AccountKey="))!;
            _accountKey = _accountKey.Substring(_accountKey.IndexOf('=')+1);
            _containers = new List<string>();
            this.InitIncludePolicy();
            //this.InitNonIncludePolicy();
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

        private void InitNonIncludePolicy()
        {
            _sasTokens = new Dictionary<string, string>();
            foreach (BlobContainerItem container in _blobServiceClient.GetBlobContainers())
            {
                if (container.Name.StartsWith(_bucketName))
                {
                    GenerateContainerSasToken(container.Name);
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

        public async Task<string> UploadFileAsync(string containerName, string blobName, Stream fileStream)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(fileStream, true);

            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                Identifier = POLICY_READONLY
            };
            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(blobClient.AccountName, _accountKey)).ToString();
            return $"{blobClient.Uri}?{sasToken}";

            //if (_sasTokens.TryGetValue(containerName,out var sasToken))
            //{
            //    return $"{blobClient.Uri}?{sasToken}";
            //}
            //sasToken = GenerateContainerSasToken(containerName);
            //return $"{blobClient.Uri}?{sasToken}";
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
                        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(2),
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
                //GenerateContainerSasToken(container);
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
