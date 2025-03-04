namespace DocumentManagement.Domain.Enums
{
    /// <summary>
    /// Defines the type of storage provider
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// Unspecified storage type
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Amazon S3 storage
        /// </summary>
        StorageTypeS3 = 1,

        /// <summary>
        /// Amazon FSx storage
        /// </summary>
        StorageTypeFsx = 2,

        /// <summary>
        /// Both S3 and FSx storage
        /// </summary>
        StorageTypeBoth = 3
    }
}