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

        // Constructor receives configuration and DbContext via DI
        public StoredFileInfoRepo(IOptions<MinioSettings> settings)
        {
            var config = settings.Value;
            _bucketName = config.BucketName;

            _minioClient = new MinioClient()
                                .WithEndpoint(config.Endpoint)
                                .WithCredentials(config.AccessKey, config.SecretKey)
                                .Build();
            EnsureBucketExistsAsync().Wait();
        }

        // Upload an IFormFile to MinIO + save metadata in DB.

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
                .WithObjectSize(file.Length) // File size in bytes
                .WithContentType(file.ContentType); // ContentType (PDF, JPG...)

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


    

