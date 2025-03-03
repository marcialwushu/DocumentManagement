namespace DocumentManagement.Domain.Enums
{
    /// <summary>
    /// Defines the type of storage provider
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// Amazon S3 storage
        /// </summary>
        S3,

        /// <summary>
        /// Amazon FSx storage
        /// </summary>
        FSx
    }
}