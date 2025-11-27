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

        public async Task<string> UploadFileAsync(IFormFile file)
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
                ID = fileName,
                Name = Path.GetFileName(file.FileName),
                Size = (int)file.Length,
                LastModifiedAt = DateTime.UtcNow,
                FilePath = fileName,
                BucketName = _bucketName,
                UploadedAt = DateTime.UtcNow,
                IsSoftDeleted = false
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

        public async Task SoftDeleteAsync (string fileName)
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
    }
}


    

