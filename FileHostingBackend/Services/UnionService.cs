using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Repos;

namespace FileHostingBackend.Services
{
    public class UnionService
    {
        private readonly IUnionRepo _unionRepo;

        public UnionService(IUnionRepo unionRepo)
        {
            _unionRepo = unionRepo;
        }

        public void CreateUnion(string unionName)
        {
            _unionRepo.CreateUnion(unionName);
        }
        public void GetUnionById(int unionId)
        {
            _unionRepo.GetUnionById(unionId);
        }
        public void UpdateUnion(string unionName)
        {
            _unionRepo.UpdateUnion(unionName);
        }
        public void DeleteUnion(string unionName)
        {
            _unionRepo.DeleteUnion(unionName);

        }

        
    }

}
