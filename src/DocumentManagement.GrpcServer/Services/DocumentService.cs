using Grpc.Core;
using DocumentManagement.GrpcServer.Proto;
using System.IO;

namespace DocumentManagement.GrpcServer.Services;

public class DocumentService : Proto.DocumentService.DocumentServiceBase
{
    private readonly ILogger<DocumentService> _logger;
    private readonly IDocumentStorageService _storageService;

    public DocumentService(ILogger<DocumentService> logger, IDocumentStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
    }

    public override async Task<UploadDocumentResponse> UploadDocument(IAsyncStreamReader<UploadDocumentRequest> requestStream, ServerCallContext context)
    {
        try
        {
            DocumentInfo? metadata = null;
            var memoryStream = new MemoryStream();

            await foreach (var request in requestStream.ReadAllAsync())
            {
                switch (request.DataCase)
                {
                    case UploadDocumentRequest.DataOneofCase.Metadata:
                        metadata = request.Metadata;
                        break;
                    case UploadDocumentRequest.DataOneofCase.Chunk:
                        await memoryStream.WriteAsync(request.Chunk.ToByteArray());
                        break;
                }
            }

            if (metadata == null)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Document metadata is required"));
            }

            memoryStream.Position = 0;
            var (documentId, docMetadata) = await _storageService.StoreDocumentAsync(
                metadata.FileName,
                metadata.ContentType,
                memoryStream,
                metadata.StorageType,
                context.CancellationToken);

            return new UploadDocumentResponse
            {
                DocumentId = documentId,
                Metadata = docMetadata
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
            if (string.IsNullOrEmpty(request.DocumentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Document ID is required"));
            }

            var (content, metadata) = await _storageService.RetrieveDocumentAsync(request.DocumentId, context.CancellationToken);

            // Send metadata first
            await responseStream.WriteAsync(new DownloadDocumentResponse
            {
                Metadata = metadata
            });

            // Stream the content in chunks
            const int chunkSize = 4096; // 4KB chunks
            var buffer = new byte[chunkSize];
            int bytesRead;

            while ((bytesRead = await content.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = new byte[bytesRead];
                Array.Copy(buffer, chunk, bytesRead);

                await responseStream.WriteAsync(new DownloadDocumentResponse
                {
                    Chunk = Google.Protobuf.ByteString.CopyFrom(chunk)
                });
            }
        }
        catch (FileNotFoundException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Document {request.DocumentId} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", request.DocumentId);
            throw new RpcException(new Status(StatusCode.Internal, "Error downloading document"));
        }
    }

    public override async Task<DeleteDocumentResponse> DeleteDocument(DeleteDocumentRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrEmpty(request.DocumentId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Document ID is required"));
            }

            await _storageService.DeleteDocumentAsync(request.DocumentId, context.CancellationToken);

            return new DeleteDocumentResponse { Success = true };
        }
        catch (FileNotFoundException)
        {
            return new DeleteDocumentResponse { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", request.DocumentId);
            throw new RpcException(new Status(StatusCode.Internal, "Error deleting document"));
        }
    }
}