using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FileHostingBackend.Repos
{
    public class UnionRepo : IUnionRepo
    {
        private readonly FileHostDBContext _dbContext;

        public UnionRepo(FileHostDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetOrCreateDefaultUnionAsync()
        {
            var existingUnion = await _dbContext.Union
                .OrderBy(u => u.UnionId)
                .FirstOrDefaultAsync();
            if (existingUnion == null)
            {
                return existingUnion.UnionId;
            }
            var deafultUnions = new Union
            {
                UnionName = "DefaultUnion"
            };
            _dbContext.Union.Add(deafultUnions);
            await _dbContext.SaveChangesAsync();
            return existingUnion.UnionId;
        }





    }
}
