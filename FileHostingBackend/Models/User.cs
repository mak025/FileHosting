using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHostingBackend.Models
{
    public abstract class User : BaseEntity
    {
        public enum UserType
        {
            Admin,
            Member,
            SysAdmin
        }

        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public Union? Union { get; set; }
        public int UnionId { get; set; } // still used as FK on User

        public UserType Type { get; set; }

        public User() : base() { }

        public User(string name, string email, string address, string phoneNumber, Union? union)
            : base(name)
        {
            Email = email;
            Address = address;
            PhoneNumber = phoneNumber;
            Union = union;
            if (union != null)
            {
                UnionId = union.UnionId;
                if (!union.Members.Contains(this))
                {
                    union.Members.Add(this);
                }
            }
        }
    }
}
