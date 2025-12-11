using FileHostingBackend.Models;
using FileHostingBackend.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Services
{
    public class UserService
    {

        private readonly IUserRepo _userRepo;

        public UserService(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public Task CreateUserAsync(string name, string email, string address, string phoneNumber, int? unionId, int userType)
        {
            return _userRepo.CreateUserAsync(name, email, address, phoneNumber, unionId, userType);
        }

        public Task<User?> GetUserByIdAsync(int userId)
        {
            return _userRepo.GetUserByIdAsync(userId);
        }

        public Task UpdateUserAsync(int userId,string name,string email, string address,string phoneNumber, int userType)
        {
            return _userRepo.UpdateUserAsync(userId, name, email, address, phoneNumber, userType);
        }

        public Task DeleteUserAsync(int userId)
        {
            return _userRepo.DeleteUserAsync(userId);
        }

    }
}
