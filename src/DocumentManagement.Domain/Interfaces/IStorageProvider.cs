using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Domain.Interfaces
{
    public interface IStorageProvider
    {
        /// <summary>
        /// Stores a file in the storage provider
        /// </summary>
        /// <param name="fileStream">The file stream to store</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="contentType">MIME type of the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The stored file's unique identifier</returns>
        Task<string> StoreAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a file from the storage provider
        /// </summary>
        /// <param name="fileId">The unique identifier of the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The file stream</returns>
        Task<Stream> GetAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file from the storage provider
        /// </summary>
        /// <param name="fileId">The unique identifier of the file</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task DeleteAsync(string fileId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the storage type of this provider
        /// </summary>
        StorageType StorageType { get; }
    }
}