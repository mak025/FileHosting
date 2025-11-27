using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
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
        private readonly MinioSettings _settings;        // MinIO configuration settings
        private readonly FileHostDBContext _dbContext;   // EF DbContext to persist file metadata

        // Constructor receives configuration and DbContext via DI
        public StoredFileInfoRepo(IOptions<MinioSettings> settings, FileHostDBContext dbContext)
        {
            var config = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _bucketName = config.BucketName ?? throw new ArgumentException("BucketName must be configured in MinioSettings");

            // Build the MinIO client using the builder-style API
            _minioClient = new MinioClient()
                                .WithEndpoint(config.Endpoint)
                                .WithCredentials(config.AccessKey, config.SecretKey)
                                .Build();
            EnsureBucketExistsAsync().Wait();
            _dbContext = dbContext;
        }

        // Upload an IFormFile to MinIO + save metadata in DB.

        private async Task EnsureBucketExistsAsync()
        {
            try 
            {
                bool exists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_settings.BucketName));
                if (!exists)
                {
                    await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName));
                }
            }
            catch (MinioException ex)
            {
                // Handle exception (log it, rethrow it, etc.)
                throw new Exception("Error ensuring bucket exists", ex);
            }

        }
        // Upload an IFormFile to MinIO + save metadata in DB.
        public async Task<string> UploadFileAsync(IFormFile file)
        {
            // Checks for null or empty file
            if (file == null || file.Length == 0) throw new ArgumentNullException(nameof(file));

            // Create a unique object name to avoid collisions in the bucket.
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            using var stream = file.OpenReadStream(); 
            var putArgs = new PutObjectArgs() 
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithStreamData(stream)
                .WithObjectSize(file.Length) // File size in bytes
                .WithContentType(file.ContentType); // ContentType (PDF, JPG...)

            await _minioClient.PutObjectAsync(putArgs);

            var metadata = new StoredFileInfo
            {
                ID = fileName,
                Name = Path.GetFileName(file.FileName),
                Size = (int)file.Length,
                LastModifiedAt = DateTime.UtcNow,
                FilePath = fileName,
                BucketName = _bucketName,
                UploadedAt = DateTime.UtcNow,
                // UploadedBy = ... // Set the user if available once user logins is fully implemented
            };
            _dbContext.StoredFiles.Add(metadata);
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


    

