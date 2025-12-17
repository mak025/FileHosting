using FileHostingBackend.Models;

namespace FileHostingBackend.Repos
{
    public interface IUnionRepo
    {
        public Task<Union> GetOrCreateDefaultUnionAsync();
        
    }
}
