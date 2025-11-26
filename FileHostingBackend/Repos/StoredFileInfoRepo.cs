using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using FileHosting.Models;
using FileHostingBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace FileHostingBackend.Repos
{
    public class StoredFileInfoRepo : IStoredFileInfoRepo
    {
        private readonly IMinioClient _minioClient;      // MinIO client used to interact with the storage server
        private readonly string _bucketName;             // Bucket to store files
        private readonly FileHostDBContext _dbContext;   // EF DbContext to persist file metadata

        // Constructor receives configuration and DbContext via DI
        public StoredFileInfoRepo(IOptions<MinioSettings> settings, FileHostDBContext dbContext)
        {
            var minioSettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _bucketName = minioSettings.BucketName ?? throw new ArgumentException("BucketName must be configured in MinioSettings");

            // Build the MinIO client using the builder-style API
            _minioClient = new MinioClient()
                .WithEndpoint(minioSettings.Endpoint)
                .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey)
                .Build();

            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Upload an IFormFile to MinIO + save metadata in DB.
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            // Create a unique object name to avoid collisions in the bucket.
            var objectName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            // Ensure bucket exists (create if missing).
            try
            {
                var exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
                if (!exists)
                {
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
                }
            }
            catch (MinioException)
            {
                // If bucket check/create fails, bubble up for the caller (or adjust logging)
                throw;
            }

            // Upload the file stream to MinIO
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            // Put object into MinIO using the stream and known length/content type
            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(file.ContentType ?? "application/octet-stream"));

            // Save metadata in EF DB so listing is simple and cheap
            var metadata = new StoredFileInfo
            {
                ID = objectName,                 // store object name as ID
                Name = Path.GetFileName(file.FileName),
                Size = (int)file.Length,
                UploadedAt = DateTime.UtcNow,
                LastModifiedAt = DateTime.UtcNow,
                FilePath = objectName,           // path in bucket
                BucketName = _bucketName,
                // UploadedBy can be set later when users are implemented
                UploadedBy = null
            };

            _dbContext.StoredFiles.Add(metadata);
            await _dbContext.SaveChangesAsync();

            // Return stored object ID (objectName) so the caller can reference the file later
            return objectName;
        }

        private async Task EnsureBucketExistsAsync()
        {
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_settings.BucketName));
            if (!found)
            {
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName));
            }
        }
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();
            var putArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putArgs);
            return fileName;
        }

        public async Task<List<StoredFileInfo>> GetAllFilesAsync()
        {
            var files = new List<StoredFileInfo>();
            var listArgs = new ListObjectsArgs().WithBucket(_bucketName);

            await foreach (var item in _minioClient.ListObjectsEnumAsync(listArgs))
            {
                files.Add(new StoredFileInfo
                {
                    Name = item.Key,
                    Size = (int)item.Size,
                    LastModifiedAt = item.LastModifiedDateTime ?? DateTime.MinValue
                });
            }

            return files;
        }

        public async Task<Stream> DownloadFileAsync(string fileName)
        {
            var memoryStream = new MemoryStream();
            var getArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithCallbackStream((stream) => stream.CopyTo(memoryStream));

            await _minioClient.GetObjectAsync(getArgs);
            memoryStream.Position = 0;
            return memoryStream;
        }
        public async Task DeleteFileAsync(string fileName)
        {
            var deleteArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);
            await _minioClient.RemoveObjectAsync(deleteArgs);
        }
    }
}


    

