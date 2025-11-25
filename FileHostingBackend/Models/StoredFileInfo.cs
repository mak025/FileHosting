using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public class StoredFileInfo
    {
        public string ID { get; set; }
        public Folder? Folder { get; set; }
        public string Name { get; set; }
        public int Size { get; set; } // Size in bytes
        public DateTime UploadedAt { get; set; }
        public User UploadedBy { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public string FilePath { get; set; } // Path in the storage system
        public string ShareLink { get; set; } // Public shareable link
        public string BucketName { get; set; } // Storage bucket name
        public StoredFileInfo() { }
    }
}
