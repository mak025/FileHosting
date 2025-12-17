using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public class StoredFileInfo
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        public int Size { get; set; } // Size in bytes
        public DateTimeOffset UploadedAt { get; set; }
        public int UploadedByID { get; set; } // Foreign key
        public User UploadedBy { get; set; }
        public DateTimeOffset LastModifiedAt { get; set; }
        public string FilePath { get; set; } // Path in the storage system
        public string? ShareLink { get; set; } = String.Empty; // Public shareable link. String.Empty default value
        public string BucketName { get; set; } // Storage bucket name
        public bool IsSoftDeleted { get; set; } = false; // Soft delete flag

        // initialize collection navigation to avoid EF confusion / null refs
        public List<User> UsersWithPermission { get; set; } = new List<User>(); // Users with access to the file

        public StoredFileInfo()
        {
            UsersWithPermission = new List<User>();
        }
    }
}