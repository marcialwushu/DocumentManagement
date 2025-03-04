using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using DocumentManagement.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace DocumentManagement.Infrastructure.Persistence
{
    public interface IDocumentRepository
    {
        Task<string> CreateDocumentAsync(Document document);
        Task<Document> GetDocumentAsync(string documentId);
        Task<IEnumerable<Document>> ListDocumentsAsync();
        Task<bool> DeleteDocumentAsync(string documentId);
    }

    public class DocumentRepository : IDocumentRepository
    {
        private readonly string _connectionString;

        public DocumentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DocumentDB") ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<string> CreateDocumentAsync(Document document)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_CreateDocument", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@FileName", document.FileName);
                    command.Parameters.AddWithValue("@Checksum", document.Checksum);
                    command.Parameters.AddWithValue("@ContentType", document.ContentType);
                    command.Parameters.AddWithValue("@FileSize", document.FileSize);
                    command.Parameters.AddWithValue("@StorageType", (int)document.StorageType);
                    command.Parameters.AddWithValue("@UploadedAt", document.UploadedAt);

                    var documentIdParam = new SqlParameter
                    {
                        ParameterName = "@DocumentId",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 50,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(documentIdParam);

                    await command.ExecuteNonQueryAsync();
                    return documentIdParam.Value.ToString();
                }
            }
        }

        public async Task<Document> GetDocumentAsync(string documentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_GetDocument", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DocumentId", documentId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Document(
                                fileName: reader.GetString(reader.GetOrdinal("FileName")),
                                checksum: reader.GetString(reader.GetOrdinal("Checksum")),
                                contentType: reader.GetString(reader.GetOrdinal("ContentType")),
                                fileSize: reader.GetInt64(reader.GetOrdinal("FileSize")),
                                storageType: (StorageType)reader.GetInt32(reader.GetOrdinal("StorageType"))
                            );
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<IEnumerable<Document>> ListDocumentsAsync()
        {
            var documents = new List<Document>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_ListDocuments", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            documents.Add(new Document(
                                fileName: reader.GetString(reader.GetOrdinal("FileName")),
                                checksum: reader.GetString(reader.GetOrdinal("Checksum")),
                                contentType: reader.GetString(reader.GetOrdinal("ContentType")),
                                fileSize: reader.GetInt64(reader.GetOrdinal("FileSize")),
                                storageType: (StorageType)reader.GetInt32(reader.GetOrdinal("StorageType"))
                            ));
                        }
                    }
                }
            }

            return documents;
        }

        public async Task<bool> DeleteDocumentAsync(string documentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("sp_DeleteDocument", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DocumentId", documentId);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }
    }
}