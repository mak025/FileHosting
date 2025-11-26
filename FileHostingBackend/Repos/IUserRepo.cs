using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Repos
{
    public interface IUserRepo
    {
        public void CreateUser(string name, string email, string address, string phoneNumber, object? union, int userType); 
        public void GetUserById(int userId);
        public void UpdateUser(int userId, string name, string email, string address, string phoneNumber, object? union, int userType);
        public void DeleteUser(int userId);




    }
}
