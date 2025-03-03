using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocumentManagement.Domain.Entities;

namespace DocumentManagement.Application.Interfaces
{
    public interface IDocumentService
    {
        /// <summary>
        /// Uploads a document to the configured storage provider(s)
        /// </summary>
        /// <param name="fileStream">The file stream to upload</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="contentType">MIME type of the file</param>
        /// <param name="storageType">Where to store the file (S3, FSx, or both)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The uploaded document's metadata</returns>
        Task<Document> UploadAsync(Stream fileStream, string fileName, string contentType, StorageType storageType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads a document by its ID
        /// </summary>
        /// <param name="documentId">The document's unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The document stream and its metadata</returns>
        Task<(Stream FileStream, Document Metadata)> DownloadAsync(Guid documentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a document's metadata by its ID
        /// </summary>
        /// <param name="documentId">The document's unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The document's metadata</returns>
        Task<Document> GetMetadataAsync(Guid documentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a document by its ID
        /// </summary>
        /// <param name="documentId">The document's unique identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteAsync(Guid documentId, CancellationToken cancellationToken = default);
    }
}