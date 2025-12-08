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
using System.IO; // required for Path

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
            await _dbContext.SaveChangesAsync();

            return fileName;
        }

        public async Task<List<StoredFileInfo>> GetAllFilesAsync()
        {
            return await _dbContext.StoredFiles
                .Where(f => !f.IsSoftDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<StoredFileInfo>> GetDeletedFilesAsync()
        {
            return await _dbContext.StoredFiles
                .Where(f => f.IsSoftDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task SoftDeleteAsync(string fileName)
        {
            var metadata = await _dbContext.StoredFiles.FirstOrDefaultAsync(f => f.FilePath == fileName);
            if (metadata != null)
            {
                metadata.IsSoftDeleted = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(string fileName)
        {
            var metadata = await _dbContext.StoredFiles.FirstOrDefaultAsync(f => f.FilePath == fileName);
            if (metadata != null && metadata.IsSoftDeleted)
            {
                metadata.IsSoftDeleted = false;
                await _dbContext.SaveChangesAsync();
            }
        }

        //public async Task PermanentlyDeleteAsync(string fileName)
        //{
        //    // Reuse existing permanent delete logic
        //    await DeleteFileAsync(fileName);
        //}

        public async Task DeleteFileAsync(string fileName)
        {
            var deleteArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);
            await _minioClient.RemoveObjectAsync(deleteArgs);

            var metadata = await _dbContext.StoredFiles.FirstOrDefaultAsync(f => f.FilePath == fileName);
            if (metadata != null)
            {
                _dbContext.StoredFiles.Remove(metadata);
                await _dbContext.SaveChangesAsync();
            }
        }

        #region Download Function
        // Prototype: return a presigned URL that the client can use to download directly from Minio
        public async Task<string> GetPresignedUrlAsync(string filePath, TimeSpan? expiry = null)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            int expires = (int)(expiry ?? TimeSpan.FromMinutes(5)).TotalSeconds;

            try
            {
                var args = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(filePath)
                    .WithExpiry(expires);

                // PresignedGetObjectAsync returns a string URL
                var url = await _minioClient.PresignedGetObjectAsync(args);
                return url;
            }
            catch (MinioException ex)
            {
                throw new Exception("Error creating presigned URL", ex);
            }
        }

        // New: server-side streaming helper that returns a MemoryStream and content-type
        public async Task<(Stream Stream, string ContentType)> GetObjectWithContentTypeAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var ms = new MemoryStream();

            try
            {
                var statArgs = new StatObjectArgs().WithBucket(_bucketName).WithObject(filePath);
                var stat = await _minioClient.StatObjectAsync(statArgs);

                var getArgs = new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(filePath)
                    .WithCallbackStream((stream) => stream.CopyTo(ms));

                await _minioClient.GetObjectAsync(getArgs);

                ms.Position = 0;
                var contentType = stat?.ContentType ?? "application/octet-stream";
                return (ms, contentType);
            }
            catch (MinioException ex)
            {
                ms.Dispose();
                throw new Exception("Error downloading object from Minio", ex);
            }
        }
        #endregion
    }
}

    

