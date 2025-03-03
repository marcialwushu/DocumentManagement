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

    public override Task DownloadDocument(DownloadDocumentRequest request, IServerStreamWriter<DownloadDocumentResponse> responseStream, ServerCallContext context)
    {
        // TODO: Implement document download logic
        throw new RpcException(new Status(StatusCode.Unimplemented, "Not implemented"));
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