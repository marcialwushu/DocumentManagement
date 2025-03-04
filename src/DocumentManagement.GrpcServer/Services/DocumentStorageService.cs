using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocumentManagement.Domain.Storage;
using DocumentManagement.GrpcServer.Proto;
using Microsoft.Extensions.Logging;

namespace DocumentManagement.GrpcServer.Services
{
    public interface IDocumentStorageService
    {
        Task<(string DocumentId, DocumentMetadata Metadata)> StoreDocumentAsync(
            string fileName,
            string contentType,
            Stream content,
            StorageType storageType,
            CancellationToken cancellationToken = default);

        Task<(Stream Content, DocumentMetadata Metadata)> RetrieveDocumentAsync(
            string documentId,
            CancellationToken cancellationToken = default);

        Task DeleteDocumentAsync(
            string documentId,
            CancellationToken cancellationToken = default);
    }

    public class DocumentStorageService : IDocumentStorageService
    {
        private readonly ILogger<DocumentStorageService> _logger;
        private readonly IStorageProvider _s3StorageProvider;
        private readonly IStorageProvider _fsxStorageProvider;

        public DocumentStorageService(
            ILogger<DocumentStorageService> logger,
            IStorageProvider s3StorageProvider,
            IStorageProvider fsxStorageProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _s3StorageProvider = s3StorageProvider ?? throw new ArgumentNullException(nameof(s3StorageProvider));
            _fsxStorageProvider = fsxStorageProvider ?? throw new ArgumentNullException(nameof(fsxStorageProvider));
        }

        public async Task<(string DocumentId, DocumentMetadata Metadata)> StoreDocumentAsync(
            string fileName,
            string contentType,
            Stream content,
            StorageType storageType,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var documentId = Guid.NewGuid().ToString();

                switch (storageType)
                {
                    case StorageType.StorageTypeS3:
                        await _s3StorageProvider.StoreDocumentAsync(documentId, fileName, contentType, content, cancellationToken);
                        break;

                    case StorageType.StorageTypeFsx:
                        await _fsxStorageProvider.StoreDocumentAsync(documentId, fileName, contentType, content, cancellationToken);
                        break;

                    case StorageType.StorageTypeBoth:
                        await Task.WhenAll(
                            _s3StorageProvider.StoreDocumentAsync(documentId, fileName, contentType, content, cancellationToken),
                            _fsxStorageProvider.StoreDocumentAsync(documentId, fileName, contentType, content, cancellationToken));
                        break;

                    default:
                        throw new ArgumentException($"Unsupported storage type: {storageType}");
                }

                var metadata = new DocumentMetadata
                {
                    DocumentId = documentId,
                    FileName = fileName,
                    ContentType = contentType,
                    StorageType = storageType,
                    UploadedAt = DateTime.UtcNow.ToString("o")
                };

                return (documentId, metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing document with type {StorageType}", storageType);
                throw;
            }
        }

        public async Task<(Stream Content, DocumentMetadata Metadata)> RetrieveDocumentAsync(
            string documentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Try S3 first, then FSx if not found
                try
                {
                    var (content, contentType, fileName) = await _s3StorageProvider.RetrieveDocumentAsync(documentId, cancellationToken);
                    var metadata = new DocumentMetadata
                    {
                        DocumentId = documentId,
                        FileName = fileName,
                        ContentType = contentType,
                        StorageType = StorageType.StorageTypeS3
                    };
                    return (content, metadata);
                }
                catch (FileNotFoundException)
                {
                    var (content, contentType, fileName) = await _fsxStorageProvider.RetrieveDocumentAsync(documentId, cancellationToken);
                    var metadata = new DocumentMetadata
                    {
                        DocumentId = documentId,
                        FileName = fileName,
                        ContentType = contentType,
                        StorageType = StorageType.StorageTypeFsx
                    };
                    return (content, metadata);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
                throw;
            }
        }

        public async Task DeleteDocumentAsync(
            string documentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to delete from both storage providers
                await Task.WhenAll(
                    DeleteFromStorageProvider(_s3StorageProvider, documentId, cancellationToken),
                    DeleteFromStorageProvider(_fsxStorageProvider, documentId, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
                throw;
            }
        }

        private async Task DeleteFromStorageProvider(
            IStorageProvider storageProvider,
            string documentId,
            CancellationToken cancellationToken)
        {
            try
            {
                await storageProvider.DeleteDocumentAsync(documentId, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                // Ignore if the file doesn't exist in this storage provider
            }
        }
    }
}