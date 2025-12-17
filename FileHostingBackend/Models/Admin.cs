namespace FileHostingBackend.Models
{
    public class Admin : User
    {
        public Admin() { }
        public Admin(string name, string email, string address, string phoneNumber, Union? union)
            : base(name, email, address, phoneNumber, union)
        {
            Type = UserType.Admin;
        }
    }
}
