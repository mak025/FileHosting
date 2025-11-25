using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public class Admin : User
    {
        public Admin() { }
        public Admin(string name, string email, string address, string phoneNumber, int unionID)
            : base(name, email, address, phoneNumber, unionID)
        {
            Type = UserType.Admin;
        }
    }
}
