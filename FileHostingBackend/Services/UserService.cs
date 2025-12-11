using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Repos;
using FileHostingBackend.Models;

namespace FileHostingBackend.Services
{
    public class UserService
    {

        private readonly IUserRepo _userRepo;

        public UserService(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }
        public void CreateUserAsync(string name, string email, string address, string phoneNumber, int? union, int userType)
        {
            _userRepo.CreateUserAsync(name, email, address, phoneNumber, union, userType);
        }

        public Task<User?> GetUserByIdAsync(int userId)
        {
            _userRepo.GetUserByIdAsync(userId);
            return _userRepo.GetUserByIdAsync(userId);
        }

        public Task UpdateUserAsync(int userId,string name,string email, string address,string phoneNumber, int userType)
        {
            return _userRepo.UpdateUserAsync(userId, name, email, address, phoneNumber, userType);
        }

        public void DeleteUserAsync(int userId)
        {
            _userRepo.DeleteUserAsync(userId);
        }

    }
}
