using FileHostingBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileHostingBackend.Repos
{
    public interface IUnionRepo
    {
        public void CreateUnion(string unionName);
        public void GetUnionById(int unionId);
        public void UpdateUnion(string unionName);
        public void DeleteUnion(int unionId);

    }
}
