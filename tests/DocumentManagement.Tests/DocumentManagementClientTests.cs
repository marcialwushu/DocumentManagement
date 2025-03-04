using Microsoft.VisualStudio.TestTools.UnitTesting;
using Grpc.Net.Client;
using DocumentManagement.GrpcServer.Services;
using DocumentManagement.Domain.Entities;
using DocumentManagement.GrpcServer.Proto;

namespace DocumentManagement.Tests
{
    [TestClass]
    public class DocumentManagementClientTests
    {
        private GrpcChannel _channel;
        private GrpcServer.Services.DocumentService.DocumentServiceClient _client;

        [TestInitialize]
        public void Setup()
        {
            // Setup the gRPC channel for testing
            _channel = GrpcChannel.ForAddress("http://localhost:5000");
            _client = new DocumentService.DocumentServiceClient(_channel);
        }

        [TestMethod]
        public async Task UploadDocument_ShouldSucceed()
        {
            // Arrange
            var documentPath = Path.Combine(Directory.GetCurrentDirectory(), "TestFiles", "TestDocument.pdf");
            var documentName = Path.GetFileName(documentPath);
            var content = await File.ReadAllBytesAsync(documentPath);

            var request = new UploadDocumentRequest
            {
                Name = documentName,
                Content = Google.Protobuf.ByteString.CopyFrom(content)
            };

            // Act
            var response = await _client.UploadDocumentAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.IsFalse(string.IsNullOrEmpty(response.DocumentId));
        }

        [TestMethod]
        public async Task GetDocument_ShouldReturnDocument()
        {
            // Arrange
            var documentId = "test-document-id";
            var request = new GetDocumentRequest { DocumentId = documentId };

            // Act
            var response = await _client.GetDocumentAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(documentId, response.DocumentId);
            Assert.IsNotNull(response.Content);
        }

        [TestMethod]
        public async Task ListDocuments_ShouldReturnDocumentsList()
        {
            // Arrange
            var request = new ListDocumentsRequest();

            // Act
            var response = await _client.ListDocumentsAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Documents);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _channel?.Dispose();
        }
    }
}