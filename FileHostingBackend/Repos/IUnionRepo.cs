using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Models;

namespace FileHostingBackend.Repos
{
    public interface IUnionRepo
    {
        public Task<Union> GetOrCreateDefaultUnionAsync();
        
    }
}
