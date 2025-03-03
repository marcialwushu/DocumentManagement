using System;

namespace DocumentManagement.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; private set; }
        public string FileName { get; private set; }
        public string Checksum { get; private set; }
        public DateTime UploadedAt { get; private set; }
        public StorageType StorageType { get; private set; }
        public string ContentType { get; private set; }
        public long FileSize { get; private set; }

        private Document() 
        { 
            // Initialize non-nullable properties with default values
            FileName = string.Empty;
            Checksum = string.Empty;
            ContentType = string.Empty;
        } // For EF Core

        public Document(
            string fileName,
            string checksum,
            string contentType,
            long fileSize,
            StorageType storageType)
        {
            Id = Guid.NewGuid();
            FileName = fileName;
            Checksum = checksum;
            ContentType = contentType;
            FileSize = fileSize;
            StorageType = storageType;
            UploadedAt = DateTime.UtcNow;
        }
    }

    public enum StorageType
    {
        S3,
        FSx,
        Both
    }
}