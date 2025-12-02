using FileHostingBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;


namespace FileHostingBackend.Repos
{
    public class StoredFileInfoRepo : IStoredFileInfoRepo
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;
        private readonly MinioSettings _settings;
        private readonly FileHostDBContext _dbContext;

        public StoredFileInfoRepo(IOptions<MinioSettings> settings, FileHostDBContext dbContext)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _bucketName = _settings.BucketName ?? throw new ArgumentException("BucketName must be configured in MinioSettings");

            _minioClient = new MinioClient()
                .WithEndpoint(_settings.Endpoint)
                .WithCredentials(_settings.AccessKey, _settings.SecretKey)
                .Build();

            // not ideal but OK for now
            EnsureBucketExistsAsync().GetAwaiter().GetResult();

            _dbContext = dbContext;
        }

        private async Task EnsureBucketExistsAsync()
        {
            try
            {
                bool exists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(_settings.BucketName));

                if (!exists)
                {
                    await _minioClient.MakeBucketAsync(
                        new MakeBucketArgs().WithBucket(_settings.BucketName));
                }
            }
            catch (MinioException ex)
            {
                throw new Exception("Error ensuring bucket exists", ex);
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, User user)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file));

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            using var stream = file.OpenReadStream();

            var putArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putArgs);

            var metadata = new StoredFileInfo
            {
                //ID = fileName,**//
                Name = Path.GetFileName(file.FileName),
                Size = (int)file.Length,
                LastModifiedAt = DateTimeOffset.UtcNow,
                FilePath = fileName,
                BucketName = _bucketName,
                UploadedAt = DateTimeOffset.UtcNow,
                IsSoftDeleted = false,
                UploadedBy = user
            };

            _dbContext.StoredFiles.Add(metadata);
            await _dbContext.SaveChangesAsync();   // 👈 important

            return fileName;
        }

        // I’d base the overview on the DB so we can filter on IsSoftDeleted etc.
        public async Task<List<StoredFileInfo>> GetAllFilesAsync()
        {
            return await _dbContext.StoredFiles
                .Where(f => !f.IsSoftDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }


        public async Task SoftDeleteAsync(string fileName)
        {

            var metadata = _dbContext.StoredFiles.FirstOrDefault(f => f.FilePath == fileName); // Find metadata by file path
            if (metadata != null) // If metadata exists, mark it as deleted
            {
                metadata.IsSoftDeleted = true; // Mark as deleted
                await _dbContext.SaveChangesAsync(); // Save changes to the database
            }


        }

        public async Task DeleteFileAsync(string fileName)
        {
            var deleteArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);
            await _minioClient.RemoveObjectAsync(deleteArgs);

            var metadata = _dbContext.StoredFiles.FirstOrDefault(f => f.FilePath == fileName); // Find metadata by file path
            if (metadata != null) // If metadata exists, remove it from the database
            {
                _dbContext.StoredFiles.Remove(metadata); // Remove metadata
                await _dbContext.SaveChangesAsync(); // Save changes to the database

            }
        }

        // New: produce a presigned GET URL so the client downloads directly from Minio
        public async Task<string> GetPresignedDownloadUrlAsync(string filePath, string downloadFileName, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            // Default expiry to 5 minutes if not provided
            var ttl = expiry ?? TimeSpan.FromMinutes(5);

            try
            {
                // Response headers tell Minio to override content-disposition/type for this request.
                // Minio will include these in the signed URL so the server enforces them when the client downloads.
                var responseHeaders = new Dictionary<string, string>
                {
                    // Make the browser treat it as an attachment with the original filename.
                    ["response-content-disposition"] = $"attachment; filename=\"{downloadFileName}\""
                };

                // If you want to set content-type explicitly, add:
                // ["response-content-type"] = "application/octet-stream" or the detected MIME type.

                var presignedArgs = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(filePath)
                    .WithExpiry((int)ttl.TotalSeconds);
                    

                // Returns the full presigned URL string.
                return await _minioClient.PresignedGetObjectAsync(presignedArgs);
            }
            catch (MinioException ex)
            {
                throw new Exception("Failed to create presigned download URL", ex);
            }
        }

    }
}


    

