using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;

namespace VoiceOfIslam.Api.Services
{
    public class BlobSasService
    {
        private readonly BlobStorageOptions _options;
        private readonly ILogger<BlobSasService> _logger;

        public BlobSasService(IOptions<BlobStorageOptions> options, ILogger<BlobSasService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public bool TryCreateReadSasUrl(string blobUrl, out string signedUrl)
        {
            signedUrl = string.Empty;

            if (string.IsNullOrWhiteSpace(blobUrl))
            {
                return false;
            }

            if (blobUrl.Contains("sig=", StringComparison.OrdinalIgnoreCase))
            {
                signedUrl = blobUrl;
                return true;
            }

            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                _logger.LogWarning("Blob storage connection string is not configured.");
                return false;
            }

            if (!TryParseBlobPath(blobUrl, out var containerName, out var blobName))
            {
                _logger.LogWarning("Could not parse blob URL: {BlobUrl}", blobUrl);
                return false;
            }

            try
            {
                var serviceClient = new BlobServiceClient(_options.ConnectionString);
                var blobClient = serviceClient
                    .GetBlobContainerClient(containerName)
                    .GetBlobClient(blobName);

                if (!blobClient.CanGenerateSasUri)
                {
                    _logger.LogWarning("Blob client cannot generate SAS URI. Check storage credentials.");
                    return false;
                }

                var expiresOn = DateTimeOffset.UtcNow.AddMinutes(Math.Clamp(_options.SasTokenMinutes, 5, 120));
                var sasBuilder = new BlobSasBuilder(BlobSasPermissions.Read, expiresOn)
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-1)
                };

                signedUrl = blobClient.GenerateSasUri(sasBuilder).ToString();
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed generating SAS URL for blob {BlobUrl}", blobUrl);
                return false;
            }
        }

        private static bool TryParseBlobPath(string blobUrl, out string containerName, out string blobName)
        {
            containerName = string.Empty;
            blobName = string.Empty;

            if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            var path = uri.AbsolutePath.Trim('/');
            var slashIndex = path.IndexOf('/');
            if (slashIndex <= 0 || slashIndex >= path.Length - 1)
            {
                return false;
            }

            containerName = path[..slashIndex];
            blobName = Uri.UnescapeDataString(path[(slashIndex + 1)..]);
            return true;
        }
    }
}
