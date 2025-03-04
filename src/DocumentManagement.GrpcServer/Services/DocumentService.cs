using Grpc.Core;
using DocumentManagement.GrpcServer.Proto;

namespace DocumentManagement.GrpcServer.Services;

public class DocumentService : Proto.DocumentService.DocumentServiceBase
{
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(ILogger<DocumentService> logger)
    {
        _logger = logger;
    }

    public override async Task<UploadDocumentResponse> UploadDocument(IAsyncStreamReader<UploadDocumentRequest> requestStream, ServerCallContext context)
    {
        try
        {
            DocumentInfo? metadata = null;
            var chunks = new List<byte[]>();

            await foreach (var request in requestStream.ReadAllAsync())
            {
                switch (request.DataCase)
                {
                    case UploadDocumentRequest.DataOneofCase.Metadata:
                        metadata = request.Metadata;
                        break;
                    case UploadDocumentRequest.DataOneofCase.Chunk:
                        chunks.Add(request.Chunk.ToByteArray());
                        break;
                }
            }

            if (metadata == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Document metadata is required"));
            }

            // TODO: Implement storage logic
            var documentId = Guid.NewGuid().ToString();

            return new UploadDocumentResponse
            {
                DocumentId = documentId,
                Metadata = new DocumentMetadata
                {
                    DocumentId = documentId,
                    FileName = metadata.FileName,
                    ContentType = metadata.ContentType,
                    StorageType = metadata.StorageType
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document upload");
            throw new RpcException(new Status(StatusCode.Internal, "Error processing document upload"));
        }
    }

    public override async Task DownloadDocument(DownloadDocumentRequest request, IServerStreamWriter<DownloadDocumentResponse> responseStream, ServerCallContext context)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(request.DocumentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Document ID is required"));
            }

            // TODO: Replace this with actual document retrieval logic
            // For now, we'll simulate a document with some sample data
            var documentId = request.DocumentId;
            var fileName = "sample.txt";
            var contentType = "text/plain";
            var content = new byte[1024]; // Simulated content
            new Random().NextBytes(content);

            // Send metadata first
            await responseStream.WriteAsync(new DownloadDocumentResponse
            {
                Metadata = new DocumentMetadata
                {
                    DocumentId = documentId,
                    FileName = fileName,
                    ContentType = contentType,
                    StorageType = StorageType.Fsx
                }
            });

            // Stream the content in chunks
            const int chunkSize = 4096; // 4KB chunks
            for (var i = 0; i < content.Length; i += chunkSize)
            {
                var chunk = new byte[Math.Min(chunkSize, content.Length - i)];
                Array.Copy(content, i, chunk, 0, chunk.Length);

                await responseStream.WriteAsync(new DownloadDocumentResponse
                {
                    Chunk = Google.Protobuf.ByteString.CopyFrom(chunk)
                });
            }

            _logger.LogInformation("Document {DocumentId} downloaded successfully", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", request.DocumentId);
            throw new RpcException(new Status(StatusCode.Internal, "Error downloading document"));
        }
    }
    

    public override Task<DocumentMetadata> GetDocumentMetadata(GetDocumentMetadataRequest request, ServerCallContext context)
    {
        // TODO: Implement metadata retrieval logic
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not implemented"));
    }

    public override Task<ListDocumentsResponse> ListDocuments(ListDocumentsRequest request, ServerCallContext context)
    {
        // TODO: Implement document listing logic
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not implemented"));
    }
}