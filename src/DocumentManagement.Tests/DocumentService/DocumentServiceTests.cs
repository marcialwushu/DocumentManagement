using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grpc.Core;
using DocumentManagement.GrpcServer.Proto;
using DocumentManagement.GrpcServer.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DocumentManagement.Tests.DocumentService
{
    [TestClass]
    public class DocumentServiceTests
    {
        private DocumentManagement.GrpcServer.Services.DocumentService _documentService;
        private Mock<ILogger<DocumentManagement.GrpcServer.Services.DocumentService>> _loggerMock;
        private Mock<IDocumentStorageService> _storageServiceMock;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<DocumentManagement.GrpcServer.Services.DocumentService>>();
            _storageServiceMock = new Mock<IDocumentStorageService>();
            _documentService = new DocumentManagement.GrpcServer.Services.DocumentService(_loggerMock.Object, _storageServiceMock.Object);
        }

        [TestMethod]
        public async Task UploadDocument_WithValidMetadataAndContent_ShouldSucceed()
        {
            // Arrange
            var metadata = new DocumentInfo
            {
                FileName = "test.txt",
                ContentType = "text/plain",
                StorageType = StorageType.Local
            };

            var content = new byte[] { 1, 2, 3, 4, 5 };
            var requests = new List<UploadDocumentRequest>
            {
                new UploadDocumentRequest { Metadata = metadata },
                new UploadDocumentRequest { Chunk = Google.Protobuf.ByteString.CopyFrom(content) }
            };

            var mockRequestStream = new MockAsyncStreamReader<UploadDocumentRequest>(requests);
            var mockContext = new Mock<ServerCallContext>().Object;

            // Act
            var response = await _documentService.UploadDocument(mockRequestStream, mockContext);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.DocumentId);
            Assert.AreEqual(metadata.FileName, response.Metadata.FileName);
            Assert.AreEqual(metadata.ContentType, response.Metadata.ContentType);
            Assert.AreEqual(metadata.StorageType, response.Metadata.StorageType);
        }

        [TestMethod]
        public async Task DownloadDocument_WithValidId_ShouldStreamContent()
        {
            // Arrange
            var request = new DownloadDocumentRequest { DocumentId = "test-id" };
            var mockResponseStream = new MockServerStreamWriter<DownloadDocumentResponse>();
            var mockContext = new Mock<ServerCallContext>().Object;

            // Act
            await _documentService.DownloadDocument(request, mockResponseStream, mockContext);

            // Assert
            Assert.IsTrue(mockResponseStream.WrittenResponses.Count > 0);
            var metadata = mockResponseStream.WrittenResponses[0].Metadata;
            Assert.IsNotNull(metadata);
            Assert.AreEqual(request.DocumentId, metadata.DocumentId);
        }
    }

    // Helper classes for testing streaming
    public class MockAsyncStreamReader<T> : IAsyncStreamReader<T>
    {
        private readonly IEnumerator<T> _enumerator;

        public MockAsyncStreamReader(IEnumerable<T> items)
        {
            _enumerator = items.GetEnumerator();
        }

        public T Current => _enumerator.Current;

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_enumerator.MoveNext());
        }
    }

    public class MockServerStreamWriter<T> : IServerStreamWriter<T>
    {
        public List<T> WrittenResponses { get; } = new List<T>();

        public Task WriteAsync(T message)
        {
            WrittenResponses.Add(message);
            return Task.CompletedTask;
        }

        public WriteOptions WriteOptions { get; set; }
    }
}