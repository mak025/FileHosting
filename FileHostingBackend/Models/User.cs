using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using FileHostingBackend.Models;
namespace FileHostingBackend.Models
{
    
    public class User
    {
        public enum UserType
        {
            // Flyt ikke rundt i rækkefølgen af listen nedenunder - giver problemer senere hvis den ændres!!
            Member = 0,
            Admin = 800,
            SysAdmin = 900
        }

        public List<StoredFileInfo> FilePermissions { get; set; } 
        public int ID { get; private set; }
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
            Name = name;
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