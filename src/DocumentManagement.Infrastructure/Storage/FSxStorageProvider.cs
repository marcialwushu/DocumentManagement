using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DocumentManagement.Domain.Storage;

namespace DocumentManagement.Infrastructure.Storage
{
    public class FSxStorageProvider : IStorageProvider
    {
        private readonly ILogger<FSxStorageProvider> _logger;
        private readonly string _basePath;

        public FSxStorageProvider(ILogger<FSxStorageProvider> logger, string basePath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> StoreDocumentAsync(string documentId, string fileName, string contentType, Stream content, CancellationToken cancellationToken = default)
        {
            try
            {
                var documentPath = Path.Combine(_basePath, documentId);
                var metadataPath = $"{documentPath}.meta";

                // Create document directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(documentPath));

                // Store the document content
                using (var fileStream = File.Create(documentPath))
                {
                    await content.CopyToAsync(fileStream, cancellationToken);
                }

                // Store metadata
                await File.WriteAllTextAsync(metadataPath, $"{fileName}|{contentType}", cancellationToken);

                _logger.LogInformation("Document {DocumentId} stored successfully at {Path}", documentId, documentPath);
                return documentPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task<(Stream Content, string ContentType, string FileName)> RetrieveDocumentAsync(string documentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var documentPath = Path.Combine(_basePath, documentId);
                var metadataPath = $"{documentPath}.meta";

                if (!File.Exists(documentPath) || !File.Exists(metadataPath))
                {
                    throw new FileNotFoundException($"Document {documentId} not found");
                }

                // Read metadata
                var metadata = await File.ReadAllTextAsync(metadataPath, cancellationToken);
                var metadataParts = metadata.Split('|');
                var fileName = metadataParts[0];
                var contentType = metadataParts[1];

                // Open file stream
                var fileStream = File.OpenRead(documentPath);

                _logger.LogInformation("Document {DocumentId} retrieved successfully", documentId);
                return (fileStream, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var documentPath = Path.Combine(_basePath, documentId);
                var metadataPath = $"{documentPath}.meta";

                if (File.Exists(documentPath))
                {
                    File.Delete(documentPath);
                }

                if (File.Exists(metadataPath))
                {
                    File.Delete(metadataPath);
                }

                _logger.LogInformation("Document {DocumentId} deleted successfully", documentId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
                throw;
            }
        }
    }
}