namespace FileHostingBackend.Models
{
    public class Member : User
    {
        public Member() { }
        public Member(string name, string email, string address, string phoneNumber, Union? union)
            : base(name, email, address, phoneNumber, union)
        {
            Type = UserType.Member;
        }
    }
}
