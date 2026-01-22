using FileHostingBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

// required for Path

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
                .WithCredentials(_settings.AccessKey, _settings.SecretKey).WithSSL(_settings.UseSSL)
                .Build();

            // not ideal but OK for now
            EnsureBucketExistsAsync().GetAwaiter().GetResult();

            _dbContext = dbContext;
        }

        private async Task EnsureBucketExistsAsync() // Ensure the Minio bucket exists
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


        public async Task<string> UploadFileAsync(IFormFile file, User user) // IFormFile from ASP.NET Core
        {
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file));

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}"; // unique file name to avoid collisions

            await using var stream = file.OpenReadStream(); // get the file stream
            var putArgs = new PutObjectArgs() // put object args
                .WithBucket(_bucketName) // bucket name
                .WithObject(fileName) // object name
                .WithStreamData(stream) // file stream
                .WithObjectSize(file.Length) // file size 
                .WithContentType(file.ContentType); // content type

            await _minioClient.PutObjectAsync(putArgs);

            var metadata = new StoredFileInfo // save metadata to database
            {
                Name = Path.GetFileName(file.FileName),
                Size = (int)file.Length,
                LastModifiedAt = DateTimeOffset.UtcNow,
                FilePath = fileName,
                BucketName = _bucketName,
                UploadedAt = DateTimeOffset.UtcNow,
                IsSoftDeleted = false,
                UploadedBy = user,
                
            };

            var isAdmin = // check if user is admin
                user.Type == FileHostingBackend.Models.User.UserType.Admin ||
                user.Type == FileHostingBackend.Models.User.UserType.SysAdmin;

            if (isAdmin) // admins get access to all files
            {
         
                var allUsers = await _dbContext.Users.ToListAsync(); // get all users
                metadata.UsersWithPermission.AddRange(allUsers); // admins get access to all files
            }
            else // regular user
            {
                metadata.UsersWithPermission.Add(user); // regular users only get access to their own files
            }

            _dbContext.StoredFiles.Add(metadata); // add to database
            await _dbContext.SaveChangesAsync();

            return fileName;
        }

        public async Task<List<StoredFileInfo>> GetAllFilesAsync() // Gets all non-deleted files
        {
            return await _dbContext.StoredFiles
                .Where(f => !f.IsSoftDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task<List<StoredFileInfo>> GetDeletedFilesAsync() // Gets all soft-deleted files
        {
            return await _dbContext.StoredFiles
                .Where(f => f.IsSoftDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task SoftDeleteAsync(string fileName) // Marks a file as soft-deleted
        {
            var metadata = await _dbContext.StoredFiles.FirstOrDefaultAsync(f => f.FilePath == fileName);
            if (metadata != null)
            {
                metadata.IsSoftDeleted = true;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task RestoreAsync(string fileName) // Restores a soft-deleted file
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

        public async Task DeleteFileAsync(string fileName) // Permanently deletes a file from Minio and database
        {
            var deleteArgs = new RemoveObjectArgs() // delete object args
                .WithBucket(_bucketName) // bucket name
                .WithObject(fileName); // object name
            await _minioClient.RemoveObjectAsync(deleteArgs);

            var metadata = await _dbContext.StoredFiles.FirstOrDefaultAsync(f => f.FilePath == fileName); // remove metadata from database
            if (metadata != null)
            {
                _dbContext.StoredFiles.Remove(metadata);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task UpdateUserPermissionsAsync(int fileId, List<int> userIds) // Update user permissions for a file
        {
            var file = await _dbContext.StoredFiles
               .Include(f => f.UsersWithPermission)
               .FirstOrDefaultAsync(f => f.ID == fileId);

            if (file == null)
                throw new Exception("Filen kunne ikke findes.");

            var selectedUsers = await _dbContext.Users
                .Where(u => userIds.Contains(u.ID))
                .ToListAsync();

            file.UsersWithPermission.Clear();
            file.UsersWithPermission.AddRange(selectedUsers);

            await _dbContext.SaveChangesAsync();
        }

        #region Download Function
        // Prototype: return a presigned URL that the client can use to download directly from Minio
        public async Task<string> GetPresignedUrlAsync(string filePath, TimeSpan? expiry = null)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            // Default expiry to 5 minutes if not provided
            int expires = (int)(expiry ?? TimeSpan.FromMinutes(5)).TotalSeconds;

            try
            {
                // args to build download link
                var args = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(filePath)
                    .WithExpiry(expires);

                // PresignedGetObjectAsync returns a string URL - calls Minio SDK to generate download link
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
                var statArgs = new StatObjectArgs().WithBucket(_bucketName).WithObject(filePath); // stat object args
                var stat = await _minioClient.StatObjectAsync(statArgs);

                var getArgs = new GetObjectArgs() // get object args
                    .WithBucket(_bucketName)
                    .WithObject(filePath)
                    .WithCallbackStream((stream) => stream.CopyTo(ms));

                await _minioClient.GetObjectAsync(getArgs);

                ms.Position = 0; // reset stream position
                var contentType = stat?.ContentType ?? "application/octet-stream"; // default content type
                return (ms, contentType);
            }
            catch (MinioException ex)
            {
                ms.Dispose();
                throw new Exception("Error downloading object from Minio", ex);
            }
        }
        #endregion
        public async Task<List<StoredFileInfo>> GetFilesWithPermissionAsync(int userId) // Gets files a user has permission to access
        {
            return await _dbContext.StoredFiles
                .Where(f => f.UsersWithPermission.Any(u => u.ID == userId) && !f.IsSoftDeleted)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }
    }
}

    

