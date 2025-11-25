using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public class Union
    {
        private static int _tempId = 0;
        public int UnionId { get; set; }
        public string UnionName { get; set; }
        public List<User> Members { get; set; } = new List<User>();
        public Union() { }
        public Union(string unionName, List<User>members)
        {
            UnionId = _tempId++;
            UnionName = unionName;
            Members = members;
        }
    }
}
