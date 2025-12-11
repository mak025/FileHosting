using FileHostingBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Models;

namespace FileHostingBackend.Repos
{
    public interface IUserRepo
    {
        Task CreateUserAsync(string name, string email, string address, string phoneNumber, int? unionId, int userType);
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateUserAsync(int userId, string name, string email, string address, string phoneNumber, int userType);
        Task DeleteUserAsync(int userId);
        Task UpdateFilePermissionsAsync(int userId, List<StoredFileInfo> files);
    }
}
