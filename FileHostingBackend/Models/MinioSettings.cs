namespace FileHostingBackend.Models

{
    public class MinioSettings
    {
        public string Endpoint { get; set; } // Minio server endpoint
        public string AccessKey { get; set; } // Access key for Minio
        public string SecretKey { get; set; } // Secret key for Minio
        public string BucketName { get; set; } // Bucket name in Minio
        public bool UseSSL { get; set; } // Whether to use SSL for Minio connection
    }
}

