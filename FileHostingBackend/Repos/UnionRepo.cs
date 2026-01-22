using FileHostingBackend.Models;
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

        public async Task<Union> GetOrCreateDefaultUnionAsync() // Ensures a default union exists and returns it
        {
            var existingUnion = await _dbContext.Union // Check for existing unions
                .OrderBy(u => u.UnionId)
                .FirstOrDefaultAsync();
            
            if (existingUnion != null)
            {
                return existingUnion;
            }
            
            var defaultUnion = new Union
            {
                UnionName = "DefaultUnion"
            };
            
            _dbContext.Add(defaultUnion);
            
            await _dbContext.SaveChangesAsync();
            
            return defaultUnion;
        }





    }
}
