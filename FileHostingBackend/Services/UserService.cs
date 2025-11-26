using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Repos;

namespace FileHostingBackend.Services
{
    public class UserService
    {

        private readonly IUserRepo _userRepo;

        public UserService(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }
        public void CreateUser(string name, string email, string address, string phoneNumber, object? union, int userType)
        {
            _userRepo.CreateUser(name, email, address, phoneNumber, union, userType);
        }

        public void GetUserById(int userId)
        {
            _userRepo.GetUserById(userId);
        }

        public void UpdateUser(int userId, string name, string email, string address, string phoneNumber, object? union, int userType)
        {
            _userRepo.UpdateUser(userId, name, email, address, phoneNumber, union, userType);
        }

        public void DeleteUser(int userId)
        {
            _userRepo.DeleteUser(userId);
        }



    }
}
