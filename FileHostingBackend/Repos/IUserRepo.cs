using FileHostingBackend.Models;
namespace FileHostingBackend.Repos
{
    public interface IUserRepo
    {
        Task CreateUserAsync(User user);
        Task DeleteUserAsync(int userId);
    }
}
