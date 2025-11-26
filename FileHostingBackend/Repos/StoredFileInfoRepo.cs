using Minio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Models;
using Microsoft.Extensions.Options;
using FileHosting.Models;

namespace FileHostingBackend.Repos
{
    public class StoredFileInfoRepo : IStoredFileInfoRepo
    {

        private readonly IMinioClient _minioClient;
        private readonly string _bucketName;

        public StoredFileInfoRepo(IOptions<MinioSettings> settings)
        {

        }


    }
}
