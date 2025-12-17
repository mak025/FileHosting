namespace FileHostingBackend.Models
{
    public class SysAdmin : User
    {
        public SysAdmin(string name, string email, string address, string phoneNumber, Union? union)
            : base(name, email, address, phoneNumber, union)
        {
            Type = UserType.SysAdmin;
        }
    }
}