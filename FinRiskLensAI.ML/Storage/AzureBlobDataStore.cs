using FinRiskLensAI.Core.Interfaces;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace FinRiskLensAI.ML.Storage
{
    /// <summary>
    /// Azure Blob Storage implementation. Each MSME gets a virtual folder named by its
    /// Udyam number inside one container; blobs are the raw source JSON files.
    /// </summary>
    public class AzureBlobDataStore : IMsmeDataStore
    {
        private readonly Lazy<BlobContainerClient> _container;

        public AzureBlobDataStore(IConfiguration configuration)
        {
            var connectionString = configuration["AzureBlob:ConnectionString"]
                ?? throw new InvalidOperationException("Missing AzureBlob:ConnectionString in configuration.");
            var containerName = configuration["AzureBlob:Container"] ?? "msme-data";

            _container = new Lazy<BlobContainerClient>(() =>
            {
                var client = new BlobContainerClient(connectionString, containerName);
                client.CreateIfNotExists();
                return client;
            });
        }

        private static string BlobPath(string uan, string fileName) => $"{Sanitize(uan)}/{fileName}";

        /// <summary>UANs are used as folder names — keep them path-safe.</summary>
        private static string Sanitize(string uan)
            => uan.Trim().Replace('\\', '-').Replace('/', '-').ToUpperInvariant();

        public async Task UploadAsync(string uan, string fileName, string content, CancellationToken ct = default)
        {
            var blob = _container.Value.GetBlobClient(BlobPath(uan, fileName));
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await blob.UploadAsync(stream, overwrite: true, ct);
        }

        public async Task<string?> DownloadAsync(string uan, string fileName, CancellationToken ct = default)
        {
            var blob = _container.Value.GetBlobClient(BlobPath(uan, fileName));
            if (!await blob.ExistsAsync(ct)) return null;
            var response = await blob.DownloadContentAsync(ct);
            return response.Value.Content.ToString();
        }

        public async Task<IReadOnlyList<string>> ListFilesAsync(string uan, CancellationToken ct = default)
        {
            var prefix = Sanitize(uan) + "/";
            var names = new List<string>();
            await foreach (var blob in _container.Value.GetBlobsAsync(prefix: prefix, cancellationToken: ct))
                names.Add(blob.Name[prefix.Length..]);
            return names;
        }

        public async Task<bool> ExistsAsync(string uan, string fileName, CancellationToken ct = default)
            => await _container.Value.GetBlobClient(BlobPath(uan, fileName)).ExistsAsync(ct);
    }
}
