using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentManagement.Domain.Storage
{
    public interface IStorageProvider
    {
        /// <summary>
        /// Stores a document in the storage system
        /// </summary>
        /// <param name="documentId">Unique identifier for the document</param>
        /// <param name="fileName">Original name of the file</param>
        /// <param name="contentType">MIME type of the content</param>
        /// <param name="content">Document content stream</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Storage location identifier</returns>
        Task<string> StoreDocumentAsync(string documentId, string fileName, string contentType, Stream content, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a document from the storage system
        /// </summary>
        /// <param name="documentId">Unique identifier for the document</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Document content stream and metadata</returns>
        Task<(Stream Content, string ContentType, string FileName)> RetrieveDocumentAsync(string documentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a document from the storage system
        /// </summary>
        /// <param name="documentId">Unique identifier for the document</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default);
    }
}