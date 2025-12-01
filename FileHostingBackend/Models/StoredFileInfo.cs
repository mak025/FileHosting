using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public class StoredFileInfo
    {
        public int ID { get; private set; }
        public Folder? Folder { get; set; }
        public string Name { get; set; }
        public int Size { get; set; } // Size in bytes
        public DateTimeOffset UploadedAt { get; set; }
        public User UploadedBy { get; set; }
        public DateTimeOffset LastModifiedAt { get; set; }
        public string FilePath { get; set; } // Path in the storage system
        public string ShareLink { get; set; } // Public shareable link
        public string BucketName { get; set; } // Storage bucket name
        public bool IsSoftDeleted { get; set; } = false; // Soft delete flag
        public StoredFileInfo() { }
    }
}
