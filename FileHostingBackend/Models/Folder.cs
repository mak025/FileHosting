using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public class Folder
    {
        public int Id { get; set; }
        public int UnionID { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string ParentFolderID { get; set; }
        public bool Visibility { get; set; }

        public Folder() { }
        public Folder 

    }
}
