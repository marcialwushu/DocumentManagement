using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DocumentManagement.Domain.Storage;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace DocumentManagement.Infrastructure.Storage
{
    public class S3StorageProvider : IStorageProvider
    {
        private readonly ILogger<S3StorageProvider> _logger;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3StorageProvider(ILogger<S3StorageProvider> logger, IAmazonS3 s3Client, string bucketName)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        }

        public async Task<string> StoreDocumentAsync(string documentId, string fileName, string contentType, Stream content, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = documentId;
                var metadataKey = $"{key}.meta";

                // Upload document content
                using (var transferUtility = new TransferUtility(_s3Client))
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = _bucketName,
                        Key = key,
                        InputStream = content,
                        ContentType = contentType
                    };

                    await transferUtility.UploadAsync(uploadRequest, cancellationToken);
                }

                // Store metadata
                var metadata = $"{fileName}|{contentType}";
                var metadataBytes = System.Text.Encoding.UTF8.GetBytes(metadata);
                using (var metadataStream = new MemoryStream(metadataBytes))
                {
                    var metadataUploadRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = _bucketName,
                        Key = metadataKey,
                        InputStream = metadataStream
                    };

                    await transferUtility.UploadAsync(metadataUploadRequest, cancellationToken);
                }

                _logger.LogInformation("Document {DocumentId} stored successfully in S3 bucket {BucketName}", documentId, _bucketName);
                return key;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing document {DocumentId} in S3", documentId);
                throw;
            }
        }

        public async Task<(Stream Content, string ContentType, string FileName)> RetrieveDocumentAsync(string documentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = documentId;
                var metadataKey = $"{key}.meta";

                // Check if document exists
                try
                {
                    await _s3Client.GetObjectMetadataAsync(_bucketName, key, cancellationToken);
                }
                catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FileNotFoundException($"Document {documentId} not found");
                }

                // Get metadata
                string fileName, contentType;
                using (var response = await _s3Client.GetObjectAsync(_bucketName, metadataKey, cancellationToken))
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    var metadata = await reader.ReadToEndAsync();
                    var metadataParts = metadata.Split('|');
                    fileName = metadataParts[0];
                    contentType = metadataParts[1];
                }

                // Get document content
                var getObjectResponse = await _s3Client.GetObjectAsync(_bucketName, key, cancellationToken);

                _logger.LogInformation("Document {DocumentId} retrieved successfully from S3 bucket {BucketName}", documentId, _bucketName);
                return (getObjectResponse.ResponseStream, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {DocumentId} from S3", documentId);
                throw;
            }
        }

        public async Task DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var key = documentId;
                var metadataKey = $"{key}.meta";

                // Delete document and metadata
                var deleteObjectsRequest = new DeleteObjectsRequest
                {
                    BucketName = _bucketName,
                    Objects = new List<KeyVersion>
                    {
                        new KeyVersion { Key = key },
                        new KeyVersion { Key = metadataKey }
                    }
                };

                await _s3Client.DeleteObjectsAsync(deleteObjectsRequest, cancellationToken);

                _logger.LogInformation("Document {DocumentId} deleted successfully from S3 bucket {BucketName}", documentId, _bucketName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {DocumentId} from S3", documentId);
                throw;
            }
        }
    }
}