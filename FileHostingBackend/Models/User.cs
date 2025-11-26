using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Models;
namespace FileHostingBackend.Models
{
    public abstract class User
    {
        public enum UserType
        {
            Admin,
            Member,
            SysAdmin
        }
        private static int _tempId = 0;
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public Union Union { get; set; }
        public int UnionId { get; set; } //maybe change when we are setting up DB context scaffolding?
        public UserType Type { get; set; }

        public User() { }

        public User(string name, string email, string address, string phoneNumber, Union union)
        {
            ID = _tempId++;
            Name = name;
            Email = email;
            Address = address;
            PhoneNumber = phoneNumber;
            Union = union;
            UnionId = union.UnionId; 

            if (union != null && !union.Members.Contains(this))
            {
                union.Members.Add(this);
            }
        }
    }
}
