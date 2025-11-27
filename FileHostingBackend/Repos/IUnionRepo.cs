using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Repos
{
    public interface IUnionRepo
    {
        public void CreateUnion(string unionName, string description);
        public void GetUnionById(int unionId);
        public void UpdateUnion(int unionId, string unionName, string description);
        public void DeleteUnion(int unionId);

    }
}
