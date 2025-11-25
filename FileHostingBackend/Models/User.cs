using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public abstract class User
    {
        public enum UserType
        {
            Admin,
            Member
        }
        private static int _tempId = 0;
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int UnionId { get; set; }
        public UserType Type { get; set; }

        public User() { }

        public User(string name, string email, string address, string phoneNumber, int unionID)
        {
            Id = _tempId++;
            Name = name;
            Email = email;
            Address = address;
            PhoneNumber = phoneNumber;
            UnionId = unionID;
        }

    }
}
