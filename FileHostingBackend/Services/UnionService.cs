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

        public void CreateUnion(string unionName, string description)
        {
            _unionRepo.CreateUnion(unionName, description);
        }
        public void GetUnionById(int unionId)
        {
            _unionRepo.GetUnionById(unionId);
        }
        public void UpdateUnion(int unionId, string unionName, string description)
        {
            _unionRepo.UpdateUnion(unionId, unionName, description);
        }
        public void DeleteUnion(int unionId)
        {
            _unionRepo.DeleteUnion(unionId);
        }
    }
}
