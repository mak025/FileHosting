using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public class Folder
    {
        private static int _tempId = 0;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Folder? ParentFolder { get; set; }
        public bool Visibility { get; set; }
        public string BucketName { get; set; }

        public Folder() { }

        public Folder (string name, string path, Folder parentFolder, string bucketName)
        {
            Id = _tempId++;
            Name = name;
            Path = path;
            ParentFolder = parentFolder;
            BucketName = bucketName;
            
            
        }
    }
}
